using DotnetEtcdProvider.Model;
using Microsoft.Extensions.Configuration;

namespace DotnetEtcdProvider;

public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Used to get etcd settings from AppSettings based on name provided as long as the data match to model `DotnetEtcdProviderConnection`
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="appSettings"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddEtcdConfiguration(
        this IConfigurationBuilder builder, string appSettings)
    {
        var tempConfig = builder.Build();
        DotnetEtcdProviderConnection connectionEtcd = tempConfig.GetSection(appSettings).Get<DotnetEtcdProviderConnection>();
        return builder.Add(new EtcdConfigurationSource(connectionEtcd));
    }
}
