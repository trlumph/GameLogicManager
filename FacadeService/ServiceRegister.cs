using System.Reflection;
using FacadeService;
using Hazelcast;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceRegister
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection)
    {
        var serviceType = typeof(Service);
        var definedTypes = serviceType.Assembly.DefinedTypes;

        var services = definedTypes
            .Where(x => x.GetTypeInfo().GetCustomAttribute<Service>() != null);

        foreach (var service in services)
        {
            serviceCollection.AddTransient(service);
        }
        
        serviceCollection.AddHttpClient("LoggingService1", client => client.BaseAddress = new Uri("http://localhost:5064/"));
        serviceCollection.AddHttpClient("LoggingService2", client => client.BaseAddress = new Uri("http://localhost:5065/"));
        serviceCollection.AddHttpClient("LoggingService3", client => client.BaseAddress = new Uri("http://localhost:5066/"));

        serviceCollection.AddSingleton<ILoggingClientService, LoggingClientService>();
        
        serviceCollection.AddHttpClient("MessagesService1", client => client.BaseAddress = new Uri("http://localhost:5074/"));
        serviceCollection.AddHttpClient("MessagesService2", client => client.BaseAddress = new Uri("http://localhost:5075/"));
        
        serviceCollection.AddSingleton<IMessagesClientService, MessagesClientService>();
        
        serviceCollection.AddSingleton<IHazelcastClient>(serviceProvider =>
        {
            var hzOptions = new HazelcastOptionsBuilder().Build();
            // Note: This call is not awaited, but it's expected to complete synchronously
            return HazelcastClientFactory.StartNewClientAsync(hzOptions).GetAwaiter().GetResult();
        });

        serviceCollection.AddEndpointsApiExplorer();
        serviceCollection.AddSwaggerGen();
        
        return serviceCollection;
    }
}