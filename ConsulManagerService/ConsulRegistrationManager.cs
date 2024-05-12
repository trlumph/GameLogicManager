using Consul;

namespace ConsulManagerService;

public class ConsulRegistrationManager
{
    public async Task Register(IConsulClient consulClient, string serviceName, string serviceId, int servicePort)
    {
        var registration = new AgentServiceRegistration()
        {
            ID = serviceId,
            Name = serviceName,
            Address = "localhost",
            Port = servicePort
        };

        await consulClient.Agent.ServiceRegister(registration);
    }

    public async Task Deregister(IConsulClient consulClient, string serviceId)
    {
        await consulClient.Agent.ServiceDeregister(serviceId);
    }
}