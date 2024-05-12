using Consul;
using ConsulManagerService;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Hazelcast;

var builder = WebApplication.CreateBuilder(args);

var consulAddress = new Uri(builder.Configuration.GetValue<string>("ConsulConfig:Host")!);

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p =>
    new ConsulClient(consulConfig =>
    {
        consulConfig.Address = consulAddress;
    }));

builder.Services.AddSingleton<ConsulRegistrationManager>();
builder.Services.AddSingleton<HazelcastConfigurationService>();

var app = builder.Build();

var consulClient = app.Services.GetRequiredService<IConsulClient>();
var hazelcastConfigService = app.Services.GetRequiredService<HazelcastConfigurationService>();
var selectedNode = await hazelcastConfigService.GetHazelcastNodeAsync();

var hzOptions = new HazelcastOptionsBuilder()
    .With((configuration, options) => 
    {
        options.Networking.Addresses.Add(selectedNode);
    })
    .Build();

var hzClient = await HazelcastClientFactory.StartNewClientAsync(hzOptions);
app.Logger.LogInformation("Connected to Hazelcast node: {SelectedNode}", selectedNode);


var serviceName = "logging-service";
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

// MySQL connection string setup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var activeUsersMap = await hzClient.GetMapAsync<string, string>("activeUsers");


app.MapPost("/register", async ([FromBody] User user) => {
    using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();
    var cmd = new MySqlCommand("INSERT INTO users (name, password) VALUES (@name, @password)", connection);
    cmd.Parameters.AddWithValue("@name", user.Name);
    cmd.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(user.Password));
    var result = await cmd.ExecuteNonQueryAsync();
    return result == 1 ? Results.Ok("User registered.") : Results.BadRequest("Registration failed.");
});

app.MapPost("/login", async ([FromBody] User user) => {
    using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();
    var cmd = new MySqlCommand("SELECT password FROM users WHERE name = @name", connection);
    cmd.Parameters.AddWithValue("@name", user.Name);
    var dbPassword = (string)await cmd.ExecuteScalarAsync();

    if (dbPassword != null && BCrypt.Net.BCrypt.Verify(user.Password, dbPassword))
    {
        var token = Guid.NewGuid().ToString();
        await activeUsersMap.SetAsync(user.Name, token);
        return Results.Ok(new { Token = token });
    }
    return Results.BadRequest("Invalid credentials.");
});

app.MapPost("/logout", async ([FromBody] TokenRequest request) => {
    var tokenRemoved = await activeUsersMap.RemoveAsync(request.Token);
    return tokenRemoved is not null ? Results.Ok("Logged out.") : Results.BadRequest("Invalid token.");
});

app.MapGet("/isOnline", async (string name) => {
    var token = await activeUsersMap.GetAsync(name);
    return token != null ? Results.Ok("Online") : Results.Ok("Offline");
});

app.Run();

record User(string Name, string Password);
record TokenRequest(string Token);