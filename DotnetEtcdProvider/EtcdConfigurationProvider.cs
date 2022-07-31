using dotnet_etcd;
using DotnetEtcdProvider.Models;
using Etcdserverpb;
using Microsoft.Extensions.Configuration;
using Timer = System.Timers.Timer;

namespace DotnetEtcdProvider;

public partial class EtcdConfigurationProvider : ConfigurationProvider
{
    private readonly DotnetEtcdProviderConnection _connectionEtcd;

    private readonly Timer _reloadTimer = new();

    private bool _etcdRequiredAuth = false;
    private readonly EtcdClient _etcdClient;
    private readonly Grpc.Core.Metadata _header;

    public EtcdConfigurationProvider(DotnetEtcdProviderConnection connectionEtcd)
    {
        ValidateConnection(connectionEtcd);

        _connectionEtcd = connectionEtcd;
        _etcdClient = new EtcdClient(_connectionEtcd.URL);
        if (_etcdRequiredAuth)
        {
            var authRes = _etcdClient.Authenticate(new Etcdserverpb.AuthenticateRequest()
            {
                Name = _connectionEtcd.Username,
                Password = _connectionEtcd.Password,
            });
            _header = new Grpc.Core.Metadata() {
                new Grpc.Core.Metadata.Entry("token",authRes.Token)
            };

        }

        if (connectionEtcd.SecondsToReload > 0)
        {
            _reloadTimer.AutoReset = false;
            _reloadTimer.Interval = TimeSpan.FromSeconds(_connectionEtcd.SecondsToReload).TotalMilliseconds;
            _reloadTimer.Elapsed += (s, e) => { Load(); };
        }
    }

    public override void Load()
    {
        try
        {
            Data = GetEtcdData();
            OnReload();
        }
        finally
        {
            if (_connectionEtcd.SecondsToReload > 0)
                _reloadTimer.Start();
        }
    }

    private IDictionary<string, string> GetEtcdData()
    {
        // Get all data from Etcd
        RangeResponse dataList = _etcdClient.GetRange("", headers: _header);

        var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var data in dataList.Kvs)
        {
            settings.Add(data.Key.ToStringUtf8(), data.Value.ToStringUtf8());
        }

        return settings;
    }
}
