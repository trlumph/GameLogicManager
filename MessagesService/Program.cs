using Hazelcast;
using MessagesService;

var builder = WebApplication.CreateBuilder(args);

var hzOptions = new HazelcastOptionsBuilder().Build();
var hzClient = await HazelcastClientFactory.StartNewClientAsync(hzOptions);
builder.Services.AddSingleton<IHazelcastClient>(hzClient);

builder.Services.AddSingleton<IMessagesManager, MessagesManager>();

var app = builder.Build();

var messagesService = app.Services.GetRequiredService<IMessagesManager>();

// Start listening to the message queue in the background
var backgroundTaskCTS = new CancellationTokenSource();
var listeningTask = messagesService.StartListening(backgroundTaskCTS.Token);

app.MapGet("/messages", async (IMessagesManager messagesManager) =>
{
    var messages = await messagesManager.GetAllMessagesAsync();
    return Results.Ok(string.Join("\n", messages));
});

app.Run();