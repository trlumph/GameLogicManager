using Hazelcast;

namespace MessagesService;

public interface IMessagesManager
{
    Task StartListening(CancellationToken cancellationToken);
    Task<List<string>> GetAllMessagesAsync();
}

public class MessagesManager : IMessagesManager
{
    private readonly IHazelcastClient _hzClient;
    private readonly List<string> _messages = new();

    public MessagesManager(IHazelcastClient hzClient)
    {
        _hzClient = hzClient;
    }

    public async Task StartListening(CancellationToken cancellationToken)
    {
        var queue = await _hzClient.GetQueueAsync<string>("messageQueue");
        while (!cancellationToken.IsCancellationRequested)
        {
            // Blocks until a message is available
            var message = await queue.TakeAsync();
            if (message is not null)
            {
                _messages.Add(message);
                Console.WriteLine($"Message received and processed: {message}");
            }
        }
    }

    public Task<List<string>> GetAllMessagesAsync()
    {
        return Task.FromResult(_messages);
    }
}