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

    private List<string> KeysBeenWatched = new();

    private bool _etcdRequiredAuth = false;
    private readonly EtcdClient _etcdClient;
    private readonly Grpc.Core.Metadata _header;

    public EtcdConfigurationProvider(DotnetEtcdProviderConnection connectionEtcd)
    {
        // Validate Connection
        ValidateConnection(connectionEtcd);

        // Setup
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

        // Init setup for schedule reload
        if (_connectionEtcd.ReloadMode == ReloadMode.ScheduledReload)
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
            if (_connectionEtcd.ReloadMode == ReloadMode.ScheduledReload)
                _reloadTimer.Start();
            else
                WatchSetupAsync();
        }
    }

    /// <summary>
    /// Get data from Etcd
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Setup Watch event to reload when there is changes
    /// </summary>
    /// <returns></returns>
    private Task WatchSetupAsync()
    {
        //foreach (var data in Data)
        //{

        //    WatchRequest request = new WatchRequest()
        //    {
        //        CreateRequest = new WatchCreateRequest()
        //        {
        //            Key = ByteString.CopyFromUtf8(data.Key)
        //        }
        //    };
        //    _etcdClient.Watch(request, updateData);
        //}

        // It will watch based on our prefix
        // For example, if we start from "AppSettingsFromEtcd:XXXX"
        // We will get latest changes as we use watch range for `AppSettingsFromEtcd`
        foreach (var prefixStr in _connectionEtcd.PrefixListUsedToWatch)
        {
            Task.Factory.StartNew(() => _etcdClient.WatchRange(prefixStr, updateData), TaskCreationOptions.LongRunning);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Update data once there are changes done
    /// </summary>
    /// <param name="response"></param>
    private void updateData(WatchEvent[] response)
    {
        foreach (WatchEvent e1 in response)
        {
            if (e1.Value == "")
                Data.Remove(e1.Key);
            else
                Data[e1.Key] = e1.Value;
        }
        OnReload();
    }
}
