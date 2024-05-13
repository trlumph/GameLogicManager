using GameService;
using MessagesService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Database.DatabaseSettings>(builder.Configuration.GetSection("GameLogDatabase"));

builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<GameManager>();

var app = builder.Build();

var gameManager = app.Services.GetRequiredService<GameManager>();
var database = app.Services.GetRequiredService<Database>();

gameManager.SetDatabase(database);

// Start listening to the message queue in the background
var loopTaskCTS = new CancellationTokenSource();
var loopTask = gameManager.HandleRequestsLoop(loopTaskCTS.Token);

app.MapPost("/killMonster", (GameManager gameManager, string playerId, string token) =>
{
    gameManager.PostKillMonsterRequest(playerId, token);
    return Results.Accepted();
});

app.MapPost("/fightPlayer", (GameManager gameManager, string playerId, string token, string opponentId) =>
{
    gameManager.PostFightPlayerRequest(playerId, token, opponentId);
    return Results.Accepted();
});

app.MapPost("/giftPlayer", (GameManager gameManager, string playerId, string token, string toPlayer, int giftAmount) =>
{
    gameManager.PostGiftPlayerRequest(playerId, token, toPlayer, giftAmount);
    return Results.Accepted();
});

app.MapGet("/logs", (GameManager gameManager) => gameManager.GetLogs());

app.Run();
