using System.Text;
using Consul;
using ConsulManagerService;

var builder = WebApplication.CreateBuilder(args);

// Configure Consul client
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => 
    new ConsulClient(consulConfig =>
{
    consulConfig.Address = new Uri(builder.Configuration.GetValue<string>("ConsulConfig:Host")!);
}));

var app = builder.Build();

app.MapPost("/register-service", async (ConsulServiceRegistration registration, IConsulClient consulClient) =>
{
    var registrationResult = await consulClient.Agent.ServiceRegister(registration.ToAgentServiceRegistration());
    return registrationResult.StatusCode == System.Net.HttpStatusCode.OK
        ? Results.Ok($"Service '{registration.ID}' registered successfully.")
        : Results.Problem("Failed to register service with Consul.");
});

app.MapGet("/discover-service/{serviceName}", async (string serviceName, IConsulClient consulClient) =>
{
    
});

app.MapGet("/config/{key}", async (string key, IConsulClient consulClient) =>
{
    var kvPair = await consulClient.KV.Get(key);
    if (kvPair.Response is null) 
        return Results.NotFound($"Configuration for key '{key}' not found.");
    
    var configValue = Encoding.UTF8.GetString(kvPair.Response.Value, 0, kvPair.Response.Value.Length);
    return Results.Ok(configValue);

});


app.Run();