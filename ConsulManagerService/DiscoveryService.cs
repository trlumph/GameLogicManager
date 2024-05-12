using Consul;

namespace ConsulManagerService;

public class DiscoveryService
{
    public static async Task<Uri?> DiscoverServiceUri(IConsulClient consulClient, string serviceName)
    {
        var services = await consulClient.Catalog.Service(serviceName);
        var response = services?.Response;
        
        if (response is null || response.Length == 0)
            return null;
        
        var random = new Random();
        var service = response.ElementAtOrDefault(random.Next(response.Length));
        
        Console.WriteLine($"Discovered {serviceName}: {string
            .Join(", ", services!
                .Response!
                .Select(s => s.ServiceAddress + ":" + s.ServicePort))}");

        if (service != null)
            return new Uri($"http://{service.ServiceAddress}:{service.ServicePort}");

        return null;
    }

}