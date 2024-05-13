using Hazelcast;

public interface IHazelcastService
{
    Task InitializeHazelcastClient(HazelcastOptions options);
    Task<string?> GetActiveUserTokenAsync(string userName);
    Task SetActiveUserTokenAsync(string userName, string token);
    Task DeleteActiveUserTokenAsync(string userName);
}