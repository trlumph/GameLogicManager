using System.Text;
using Consul;

namespace ConsulManagerService;

public class MessageQueueConfigurationService
{
    private readonly IConsulClient _consulClient;
    private readonly string _mqConfigKey;

    public MessageQueueConfigurationService(IConsulClient consulClient, IConfiguration configuration)
    {
        _consulClient = consulClient;
        _mqConfigKey = configuration.GetValue<string>("HazelcastConfig:MQNameKey")!;
    }

    public async Task<string> GetMQNameAsync()
    {
        var queueNamePair = await _consulClient.KV.Get(_mqConfigKey);
        var name = queueNamePair?.Response?.Value ?? throw new InvalidOperationException("MQ configuration not found in Consul.");
        var queueName = Encoding.UTF8.GetString(name);

        return queueName;
    }
}
