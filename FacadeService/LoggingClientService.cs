namespace FacadeService;

public interface ILoggingClientService
{
    Task<HttpClient> GetClientAsync();
}

public class LoggingClientService : ILoggingClientService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly string[] _clientNames = { "LoggingService1", "LoggingService2", "LoggingService3" };
    private readonly Random _random = new();

    public LoggingClientService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<HttpClient> GetClientAsync()
    {
        var clientName = _clientNames[_random.Next(_clientNames.Length)];
        var client = _clientFactory.CreateClient(clientName);
        return Task.FromResult(client);
    }
}
