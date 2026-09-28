using Api;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Api.Tests;

// A real Postgres 17 in a throwaway container per test class — no mocks, no shared DB (docs 40)
public class NotesDbTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public Task InitializeAsync() => _postgres.StartAsync();

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    private AppDb CreateDb() =>
        new(new DbContextOptionsBuilder<AppDb>().UseNpgsql(_postgres.GetConnectionString()).Options);

    [Fact]
    public async Task Migrations_apply_and_notes_round_trip()
    {
        await using (var db = CreateDb())
        {
            await db.Database.MigrateAsync();               // the same migrations the migrator runs
            db.Notes.Add(new Note { Text = "from a test", CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        await using (var db = CreateDb())
        {
            var note = await db.Notes.SingleAsync();
            Assert.Equal("from a test", note.Text);
        }
    }
}
