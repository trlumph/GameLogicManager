using System.Text;
using Consul;

namespace ConsulManagerService;

public class HazelcastConfigurationService
{
    private readonly IConsulClient _consulClient;
    private readonly string _nodesKey;

    public HazelcastConfigurationService(IConsulClient consulClient, IConfiguration configuration)
    {
        _consulClient = consulClient;
        _nodesKey = configuration.GetValue<string>("HazelcastConfig:NodesKey")!;
    }

    public async Task<string> GetHazelcastNodeAsync()
    {
        var getPair = await _consulClient.KV.Get(_nodesKey);
        if (getPair.Response?.Value == null)
        {
            throw new InvalidOperationException($"Hazelcast nodes configuration not found for key: {_nodesKey}");
        }

        var nodesList = Encoding.UTF8.GetString(getPair.Response.Value);
        var hazelcastNodes = nodesList.Split(',');
        if (!hazelcastNodes.Any())
        {
            throw new InvalidOperationException("No Hazelcast nodes configured.");
        }

        var random = new Random();
        return hazelcastNodes[random.Next(hazelcastNodes.Length)];
    }
}
