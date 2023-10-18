﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_etcd;
using DotnetEtcdProvider.Extensions;
using DotnetEtcdProvider.Models;
using Etcdserverpb;
using Microsoft.Extensions.Configuration;
using Timer = System.Timers.Timer;

namespace DotnetEtcdProvider
{
    public partial class EtcdConfigurationProvider : ConfigurationProvider
    {
        private readonly DotnetEtcdProviderConnection _connectionEtcd;

        private readonly Timer _reloadTimer = new Timer();

        private bool _etcdRequiredAuth = false;

        private readonly EtcdClient _etcdClient;

        private readonly Grpc.Core.Metadata _header;

        public EtcdConfigurationProvider(DotnetEtcdProviderConnection connectionEtcd)
        {
            // Validate Connection
            ValidateConnection(connectionEtcd);

            // Setup
            _connectionEtcd = connectionEtcd;
            _etcdClient = SetupEtcdClient();
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

        private EtcdClient SetupEtcdClient()
        {
            if (IsValidSingleEndpoint(_connectionEtcd.URL))
            {

                return new EtcdClient(_connectionEtcd.URL);
            }
            else
            {
                string urls = String.Join(",", _connectionEtcd.URLs);
                return new EtcdClient(urls);
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
                var key = data.Key.ToStringUtf8();
                var val = data.Value.ToStringUtf8();
                if (!val.IsEmpty())
                {
                    val = val.Trim();
                    key = MapKeyToConfigurationProviderKeyPattern(key);
                    if (IsValueAnArray(val) || IsValueObject(val))
                    {
                        Dictionary<string, string> result = ConvertDynamicStringToDictionary(key, val);
                        foreach (var valDic in result)
                        {
                            settings.Add(valDic.Key, valDic.Value);
                        }
                    }
                    else
                    {

                        settings.Add(key, val);
                    }
                }
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
                Task.Factory.StartNew(() => _etcdClient.WatchRange(prefixStr, UpdateData, headers: _header), TaskCreationOptions.LongRunning);
            }

            return Task.CompletedTask;
        }

        // // -------------------------------
        // // Print function that prints key and value from the watch response
        // private static void UpdateData(WatchResponse response)
        // {
        //     if (response.Events.Count == 0)
        //     {
        //         Console.WriteLine(response);
        //     }
        //     else
        //     {
        //         ReloadData(response.Events.);
        //     }
        //     Console.WriteLine("UpdateData response");
        //     Console.WriteLine(JsonSerializer.Serialize(response));
        // }


        /// <summary>
        /// Update data once there are changes done
        /// </summary>
        /// <param name="response"></param>
        private void UpdateData(WatchEvent[] response)
        {
            // Prevent the function make task crash due to the data cannot convert to proper data type
            try
            {
                foreach (WatchEvent e1 in response)
                {
                    string key = MapKeyToConfigurationProviderKeyPattern(e1.Key);
                    if (e1.Value.IsEmpty())
                        Data.Remove(key);
                    else
                    {
                        string value = e1.Value.Trim();
                        if (IsValueAnArray(value) || IsValueObject(value))
                        {
                            Dictionary<string, string> result = ConvertDynamicStringToDictionary(key, value);

                            // Delete Old keys
                            List<string> oldKeys = Data.Where(u => u.Key.StartsWith(key)).Select(u => u.Key).ToList();
                            foreach (var oldKey in oldKeys)
                                Data.Remove(oldKey);

                            // Insert New Data
                            foreach (var valDic in result)
                            {
                                Data[valDic.Key] = valDic.Value;
                            }
                        }
                        else
                        {
                            Data[key] = e1.Value;
                        }
                    }
                }
                OnReload();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}
