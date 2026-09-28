var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();

var app = builder.Build();

// Which container answered + which environment config it sees
app.MapGet("/", () => new
{
    message = app.Configuration["Greeting"] ?? "Hello from Docker",
    machine = Environment.MachineName,          // = container ID (short)
    environment = app.Environment.EnvironmentName
});

// What the .NET runtime sees inside cgroup limits (see docs 13-limits)
app.MapGet("/limits", () => new
{
    gcHeapLimitMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024,
    cpus = Environment.ProcessorCount
});

// Simulated memory leak: holds N MB until the container restarts (see docs 13-limits)
//   managed (default) → GC heap limit → OutOfMemoryException, process survives
//   ?native=true      → outside the GC  → kernel OOM killer, exit 137
var managed = new List<byte[]>();
var nativeMB = 0;
var ones = Enumerable.Repeat((byte)1, 1024 * 1024).ToArray();
app.MapPost("/allocate/{mb:int}", (int mb, bool native = false) =>
{
    for (var i = 0; i < mb; i++)
    {
        if (native)
        {
            var ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(ones.Length);
            System.Runtime.InteropServices.Marshal.Copy(ones, 0, ptr, ones.Length);  // touch pages
            nativeMB++;
        }
        else
        {
            managed.Add((byte[])ones.Clone());
        }
    }
    return new { managedMB = managed.Count, nativeMB };
});

app.MapHealthChecks("/health");

app.Run();
