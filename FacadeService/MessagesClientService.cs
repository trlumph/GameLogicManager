namespace FacadeService;

public interface IMessagesClientService
{
    Task<HttpClient> GetClientAsync();
}

public class MessagesClientService : IMessagesClientService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly string[] _clientNames = { "MessagesService1", "MessagesService2" };
    private readonly Random _random = new();

    public MessagesClientService(IHttpClientFactory clientFactory)
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
