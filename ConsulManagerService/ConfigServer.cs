using System.Text;
using Consul;

namespace ConsulManagerService;

public class ConfigServer
{
    public async Task<string?> GetConsulConfigValue(IConsulClient consulClient, string key)
    {
        var getPair = await consulClient.KV.Get(key);
        return getPair?.Response != null ? Encoding.UTF8.GetString(getPair.Response.Value) : null;
    }
}