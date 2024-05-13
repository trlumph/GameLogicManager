using Hazelcast;
using Hazelcast.DistributedObjects;

public class HazelcastService : IHazelcastService
{
    private IHazelcastClient _hazelcastClient;
    private IHMap<string, string> _activeUsersMap;

    public async Task InitializeHazelcastClient(HazelcastOptions options)
    {
        _hazelcastClient = await HazelcastClientFactory.StartNewClientAsync(options);
        _activeUsersMap = await _hazelcastClient.GetMapAsync<string, string>("activeUsers");
    }

    public async Task<string?> GetActiveUserTokenAsync(string userName)
    {
        return await _activeUsersMap.GetAsync(userName);
    }

    public async Task SetActiveUserTokenAsync(string userName, string token)
    {
        await _activeUsersMap.SetAsync(userName, token);
    }

    public async Task DeleteActiveUserTokenAsync(string userName)
    {
        await _activeUsersMap.DeleteAsync(userName);
    }
}