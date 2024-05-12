using FacadeService;
using Hazelcast;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseSwagger();  
app.UseSwaggerUI();

app.MapPost("/facade", async (
    MessageRequest? request,
    ILoggingClientService loggingClientService,
    IHazelcastClient hzClient) =>
{
    if (request is null) 
        return Results.BadRequest("Message is required");
    
    var queue = await hzClient.GetQueueAsync<string>("messageQueue");
    await queue.OfferAsync(request.Message);

    var client = await loggingClientService.GetClientAsync();
    var uuid = Guid.NewGuid().ToString();
    await client.PostAsJsonAsync("/log", new { Uuid = uuid, request.Message });
    return Results.Ok(new { Uuid = uuid });
});

app.MapGet("/facade", async (
    ILoggingClientService loggingClientService,
    IMessagesClientService messagesClientService) =>
{
    var messagesClient = await messagesClientService.GetClientAsync();
    var messagesResponse = await messagesClient.GetStringAsync("/messages");

    var loggingClient = await loggingClientService.GetClientAsync();
    var loggingResponse = await loggingClient.GetStringAsync("/log");
    return Results.Ok($"{loggingResponse}\n{messagesResponse}");
});

app.Run();

record MessageRequest(string Message);