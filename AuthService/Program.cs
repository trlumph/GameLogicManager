using Consul;
using ConsulManagerService;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Hazelcast;

var builder = WebApplication.CreateBuilder(args);

var consulAddress = new Uri(builder.Configuration.GetValue<string>("ConsulConfig:Host")!);

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p =>
    new ConsulClient(consulConfig => { consulConfig.Address = consulAddress; }));

builder.Services.AddSingleton<ConsulRegistrationManager>();
builder.Services.AddSingleton<HazelcastConfigurationService>();

var app = builder.Build();

var consulClient = app.Services.GetRequiredService<IConsulClient>();
var hazelcastConfigService = app.Services.GetRequiredService<HazelcastConfigurationService>();
var selectedNode = await hazelcastConfigService.GetHazelcastNodeAsync();

var hzOptions = new HazelcastOptionsBuilder()
    .With((configuration, options) => { options.Networking.Addresses.Add(selectedNode); })
    .Build();
hzOptions.ClusterName = "hello-world";

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

// MySQL connection string setup
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
const string ConnectionString = "server=127.0.0.1;uid=root;pwd=admin;database=mydb";
var activeUsersMap = await hzClient.GetMapAsync<string, string>("activeUsers");

var client = new HttpClient();

app.MapPost("/register", async ([FromBody] User user) =>
{
    await using var connection = new MySqlConnection(ConnectionString);
    await connection.OpenAsync();
    var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE name = @name", connection);
    checkCmd.Parameters.AddWithValue("@name", user.Name);
    var count = (long)(await checkCmd.ExecuteScalarAsync())!;
    if (count > 0) return Results.BadRequest("User already exists.");

    var addCmd = new MySqlCommand("INSERT INTO users (name, password) VALUES (@name, @password)", connection);
    addCmd.Parameters.AddWithValue("@name", user.Name);
    var hash = BCrypt.Net.BCrypt.HashPassword(user.Password);
    addCmd.Parameters.AddWithValue("@password", hash);
    var result = await addCmd.ExecuteNonQueryAsync();
    if (result == 0) return Results.BadRequest("Registration failed.");

    await client.PostAsync($"http://localhost:8181/scores/user/{user.Name}", new StringContent(""));
    return Results.Ok("Registered.");
});

app.MapPost("/login", async ([FromBody] User user) =>
{
    var activeToken = await activeUsersMap.GetAsync(user.Name);
    if (activeToken != null) return Results.BadRequest("User already logged in.");

    await using var connection = new MySqlConnection(ConnectionString);
    await connection.OpenAsync();
    var cmd = new MySqlCommand("SELECT password FROM users WHERE name = @name", connection);
    cmd.Parameters.AddWithValue("@name", user.Name);
    var dbPassword = (string?)await cmd.ExecuteScalarAsync();

    if (dbPassword != null && BCrypt.Net.BCrypt.Verify(user.Password, dbPassword))
    {
        var token = Guid.NewGuid().ToString();
        await activeUsersMap.SetAsync(user.Name, token);
        return Results.Ok(new {Token = token});
    }
    return Results.BadRequest("Invalid credentials.");
});

app.MapPost("/logout", async ([FromBody] LogoutRequest request) =>
{
    var token = await activeUsersMap.GetAsync(request.Name);
    if (token == null) return Results.BadRequest("User not logged in.");
    if (token != request.Token) return Results.BadRequest("Invalid token.");

    await activeUsersMap.DeleteAsync(request.Name);
    return Results.Ok("Logged out.");
});

app.MapGet("/isOnline", async (string name) =>
{
    var token = await activeUsersMap.GetAsync(name);
    return token != null ? Results.Ok("Online") : Results.Ok("Offline");
});

app.MapGet("/validate", async (string name, string token) =>
{
    var activeToken = await activeUsersMap.GetAsync(name);
    return activeToken == token ? Results.Ok("Valid") : Results.BadRequest("Invalid");
});

app.Run();

record User(string Name, string Password);

record LogoutRequest(string Name, string Token);