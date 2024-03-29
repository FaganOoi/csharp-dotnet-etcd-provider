﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DotnetEtcdProvider
{
    public partial class EtcdConfigurationProvider
    {
        private static Dictionary<string, string> ConvertDynamicStringToDictionary(string key, string val)
        {

            var settings = new Dictionary<string, string>();
            try
            {
                if (!IsValueAnArray(val) && !IsValueObject(val))
                {
                    settings.Add(key, val);
                    return settings;
                }

                using (JsonDocument document = JsonDocument.Parse(val))
                {
                    JsonElement root = document.RootElement;

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        int a = 0;
                        foreach (JsonElement item in root.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Object)
                            {
                                ConvertObjectStringToDictionary($"{key}:{a}", settings, item);
                            }
                            else
                            {
                                string keyObject = $"{key}:{a}";
                                settings.Add(keyObject, item.ToString());
                            }
                            a++;
                        }
                    }
                    else if (root.ValueKind == JsonValueKind.Object)
                    {
                        ConvertObjectStringToDictionary(key, settings, root);
                    }
                    else
                    {

                        settings.Add(key, root.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return settings;
        }

        private static void ConvertObjectStringToDictionary(string key, Dictionary<string, string> settings, JsonElement item)
        {
            foreach (JsonProperty property in item.EnumerateObject())
            {
                string keyTmp = property.Name;
                string value = property.Value.ToString();
                string currentKeyObject = $"{key}:{keyTmp}";

                Dictionary<string, string> result = ConvertDynamicStringToDictionary(currentKeyObject, value);
                foreach (var valDic in result)
                {
                    settings.Add(valDic.Key, valDic.Value);
                }

            }
        }

        /// <summary>
        /// Handle etcd ui which will use slash to split the node or folder
        /// </summary>
        /// <param name="originalKey"></param>
        /// <returns></returns>
        private string MapKeyToConfigurationProviderKeyPattern(string originalKey)
        {
            if (_connectionEtcd.PrefixListUsedToRemoveInData.Any())
            {
                foreach (var pref in _connectionEtcd.PrefixListUsedToRemoveInData)
                {
                    if (originalKey.StartsWith(pref))
                    {
                        originalKey = originalKey.Remove(0, pref.Length);
                        break;
                    }
                }
            }

            if (originalKey.StartsWith("/"))
                originalKey = originalKey.Substring(1);

            return originalKey.Replace("/", ":");

        }

    }
}
