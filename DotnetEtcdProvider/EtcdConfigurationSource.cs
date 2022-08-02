using DotnetEtcdProvider.Models;
using Microsoft.Extensions.Configuration;

namespace DotnetEtcdProvider
{
    public class EtcdConfigurationSource : IConfigurationSource
    {
        private readonly DotnetEtcdProviderConnection _connectionEtcd;

        public EtcdConfigurationSource(DotnetEtcdProviderConnection connectionEtcd) =>
            _connectionEtcd = connectionEtcd;

        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new EtcdConfigurationProvider(_connectionEtcd);
    }
}