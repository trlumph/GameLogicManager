using Hazelcast;


var builder = WebApplication.CreateBuilder(args);

// Initialize Hazelcast
var hazelcastNode = Environment.GetEnvironmentVariable("HAZELCAST_NODE");

var hzOptions = new HazelcastOptionsBuilder()
    .With((configuration, options) =>
    {
        options.Networking.Addresses.Clear();
        if (!string.IsNullOrEmpty(hazelcastNode))
        {
            options.Networking.Addresses.Add(hazelcastNode);
        }
    })
    .Build();

var hzClient = await HazelcastClientFactory.StartNewClientAsync(hzOptions);
var map = await hzClient.GetMapAsync<string, string>("messages");
Console.WriteLine($"Running on {hazelcastNode} Hazelcast node");

var app = builder.Build();

app.MapPost("log", async (LogEntry logEntry) => {
    await map.SetAsync(logEntry.Uuid, logEntry.Message);
    Console.WriteLine(logEntry.Message);
    return Results.Ok();
});

app.MapGet("log", async () => {
    var allMessages = await map.GetValuesAsync();
    return Results.Ok(string.Join("\n", allMessages));
});

app.Run();

record LogEntry(string Uuid, string Message);