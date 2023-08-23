using System;
using System.Collections.Generic;
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


                if (IsValueAnArray(value) || IsValueObject(value)) // if object inside still got array or object
                {
                    Dictionary<string, string> result = ConvertDynamicStringToDictionary(currentKeyObject, value);
                    foreach (var valDic in result)
                    {
                        settings.Add(valDic.Key, valDic.Value);
                    }
                }
                else
                {
                    settings.Add(currentKeyObject, value);
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
            if (originalKey.StartsWith("/"))
                originalKey = originalKey.Substring(1);

            return originalKey.Replace("/", ":");

        }

    }
}
