using Consul;
using ConsulManagerService;
using Microsoft.AspNetCore.Mvc;
using Hazelcast;

var builder = WebApplication.CreateBuilder(args);

var consulAddress = new Uri(builder.Configuration.GetValue<string>("ConsulConfig:Host")!);

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p =>
    new ConsulClient(consulConfig => { consulConfig.Address = consulAddress; }));

builder.Services.AddSingleton<ConsulRegistrationManager>();
builder.Services.AddSingleton<HazelcastConfigurationService>();

// Register the UserRepository
builder.Services.AddTransient<IUserRepository, UserRepository>(sp =>
    new UserRepository(builder.Configuration.GetConnectionString("DefaultConnection")!));

// Register the HazelcastService
builder.Services.AddSingleton<IHazelcastService, HazelcastService>();

// Register the EncryptionService
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

var app = builder.Build();

var consulClient = app.Services.GetRequiredService<IConsulClient>();
var hazelcastConfigService = app.Services.GetRequiredService<HazelcastConfigurationService>();
var selectedNode = await hazelcastConfigService.GetHazelcastNodeAsync();

var hzOptions = new HazelcastOptionsBuilder()
    .With((configuration, options) => { options.Networking.Addresses.Add(selectedNode); })
    .Build();
hzOptions.ClusterName = builder.Configuration.GetValue<string>("HazelcastConfig:GroupKey");

app.Logger.LogInformation("Connecting to Hazelcast node: {SelectedNode}", selectedNode);
var hzClient = await HazelcastClientFactory.StartNewClientAsync(hzOptions);
app.Logger.LogInformation("Connected to Hazelcast node: {SelectedNode}", selectedNode);

var serviceName = "auth-service";
var serviceId = $"{serviceName}-{Guid.NewGuid()}";
var servicePort = new Uri(builder.Configuration.GetValue<string>("ASPNETCORE_URLS")!).Port;

var consulManager = app.Services.GetRequiredService<ConsulRegistrationManager>();
await consulManager.Register(consulClient, serviceName, serviceId, servicePort);

app.Lifetime.ApplicationStopping.Register(async () =>
{
    await consulManager.Deregister(consulClient, serviceId);
    await hzClient.DisposeAsync();
});

AppDomain.CurrentDomain.UnhandledException += async (sender, eventArgs) =>
{
    Console.WriteLine("Unhandled exception occurred. Deregistering service...");
    await consulManager.Deregister(consulClient, serviceId);
};

// Register the HazelcastClient in the HazelcastService
var hazelcastService = app.Services.GetRequiredService<IHazelcastService>();
await hazelcastService.InitializeHazelcastClient(hzOptions);

var client = new HttpClient();

app.MapPost("/register", async ([FromBody] User user, [FromServices] IUserRepository userRepository, [FromServices] IEncryptionService encryptionService) =>
{
    var userExists = await userRepository.UserExistsAsync(user.Name);
    if (userExists) return Results.BadRequest("User already exists.");

    var hash = encryptionService.HashPassword(user.Password);
    var result = await userRepository.AddUserAsync(user.Name, hash);
    if (!result) return Results.BadRequest("Registration failed.");

    await client.PostAsync($"http://localhost:8181/scores/user/{user.Name}", new StringContent(""));
    return Results.Ok("Registered.");
});

app.MapPost("/login", async ([FromBody] User user, [FromServices] IUserRepository userRepository, [FromServices] IHazelcastService hazelcastService, [FromServices] IEncryptionService encryptionService) =>
{
    var activeToken = await hazelcastService.GetActiveUserTokenAsync(user.Name);
    if (activeToken != null) return Results.BadRequest("User already logged in.");

    var dbPassword = await userRepository.GetUserPasswordAsync(user.Name);
    if (dbPassword != null && encryptionService.VerifyPassword(user.Password, dbPassword))
    {
        var token = Guid.NewGuid().ToString();
        await hazelcastService.SetActiveUserTokenAsync(user.Name, token);
        return Results.Ok(new { Token = token });
    }
    return Results.BadRequest("Invalid credentials.");
});

app.MapPost("/logout", async ([FromBody] LogoutRequest request, [FromServices] IHazelcastService hazelcastService) =>
{
    var token = await hazelcastService.GetActiveUserTokenAsync(request.Name);
    if (token == null) return Results.BadRequest("User not logged in.");
    if (token != request.Token) return Results.BadRequest("Invalid token.");

    await hazelcastService.DeleteActiveUserTokenAsync(request.Name);
    return Results.Ok("Logged out.");
});

app.MapGet("/isOnline", async (string name, [FromServices] IHazelcastService hazelcastService) =>
{
    var token = await hazelcastService.GetActiveUserTokenAsync(name);
    return token != null ? Results.Ok("Online") : Results.Ok("Offline");
});

app.MapGet("/validate", async (string name, string token, [FromServices] IHazelcastService hazelcastService) =>
{
    var activeToken = await hazelcastService.GetActiveUserTokenAsync(name);
    return activeToken == token ? Results.Ok("Valid") : Results.BadRequest("Invalid");
});

app.Run();

record User(string Name, string Password);

record LogoutRequest(string Name, string Token);