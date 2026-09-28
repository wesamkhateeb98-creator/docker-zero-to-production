using System.Text.Json;
using Api;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Docker/Compose secrets: file /run/secrets/ConnectionStrings__Db → config key ConnectionStrings:Db (docs 26)
builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);

builder.Services.AddDbContext<AppDb>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Db")));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

// Without a persisted key ring, every redeploy invalidates cookies/antiforgery tokens (docs 44)
if (builder.Configuration["DataProtection:KeysPath"] is { } keysPath)
    builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysPath));

builder.Services.AddHealthChecks()
    .AddCheck<DbHealthCheck>("db")
    .AddCheck<RedisHealthCheck>("redis");

var app = builder.Build();

app.MapGet("/", () => new
{
    service = "notes-api",
    version = app.Configuration["APP_VERSION"] ?? "dev",   // set at build time → see which image is live
    machine = Environment.MachineName
});

// Cache-aside: Redis first, Postgres on miss (docs 19)
app.MapGet("/notes", async (AppDb db, IConnectionMultiplexer redis, HttpResponse res) =>
{
    var cache = redis.GetDatabase();
    if (await cache.StringGetAsync("notes") is { HasValue: true } hit)
    {
        res.Headers["X-Cache"] = "HIT";
        return Results.Content(hit.ToString(), "application/json");
    }

    var notes = await db.Notes.OrderBy(n => n.Id).ToListAsync();
    var json = JsonSerializer.Serialize(notes, JsonSerializerOptions.Web);
    await cache.StringSetAsync("notes", json, TimeSpan.FromSeconds(30));
    res.Headers["X-Cache"] = "MISS";
    return Results.Content(json, "application/json");
});

app.MapPost("/notes", async (NoteInput input, AppDb db, IConnectionMultiplexer redis) =>
{
    var note = new Note { Text = input.Text, CreatedAt = DateTime.UtcNow };
    db.Notes.Add(note);
    await db.SaveChangesAsync();
    await redis.GetDatabase().KeyDeleteAsync("notes");        // invalidate cache
    return Results.Created($"/notes/{note.Id}", note);
});

app.MapHealthChecks("/health");

app.Run();

record NoteInput(string Text);
