using Consul;

namespace ConsulManagerService;

public record ConsulServiceRegistration(string ID, string Name, string Address, int Port)
{
    public AgentServiceRegistration ToAgentServiceRegistration() => new()
    {
        ID = ID,
        Name = Name,
        Address = Address,
        Port = Port
    };
}