using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DotnetEtcdProvider.Exceptions;
using DotnetEtcdProvider.Extensions;
using DotnetEtcdProvider.Models;

namespace DotnetEtcdProvider
{
    public partial class EtcdConfigurationProvider
    {
        private delegate string StringReturnAction();

        private void ValidateConnection(DotnetEtcdProviderConnection connection)
        {
            List<string> errorMessages = new List<string>();
            HandleValidation(() => ValidateUrl(connection.URL, connection.URLs), errorMessages);
            HandleValidation(() => ValidateAuth(connection.Username, connection.Password), errorMessages);
            HandleValidation(() => ValidateReloadMode(connection), errorMessages);

            if (errorMessages.Count > 0)
                throw new EtcdProviderConfigurationException(string.Join(',', errorMessages), connection);
        }

        private void HandleValidation(StringReturnAction action, List<string> errorMsgs)
        {
            string result = action.Invoke();
            if (result.HasData())
            {
                errorMsgs.Add(result);
            }
        }

        private string ValidateUrl(string url, List<string> urls)
        {
            if (IsValidSingleEndpoint(url) || IsValidMultipleEndpoints(urls))
                return null;
            return "Please provided valid connection to access Etcd either by URL or URLs field";

        }

        private static bool IsValidSingleEndpoint(string url)
        {
            return url.HasData() && url.IsUrlWithProxy();
        }

        private static bool IsValidMultipleEndpoints(List<string> urls)
        {
            return urls != null && urls.Any() && urls.All(u => u.IsUrlWithProxy());
        }

        private string ValidateAuth(string username, string password)
        {
            if (HasUsernameButEmptyPassword(username, password) || HasPasswordButEmptyUsername(username, password))
                return "Please check on username and password provided";

            return null;

            static bool HasUsernameButEmptyPassword(string username, string password)
            {
                return username.IsEmpty() && !password.IsEmpty();
            }

            static bool HasPasswordButEmptyUsername(string username, string password)
            {
                return !username.IsEmpty() && password.IsEmpty();
            }
        }

        private static bool IsAuthenticationNeeded(string username, string password)
        {
            return username.HasData() && password.HasData();
        }

        private string ValidateReloadMode(DotnetEtcdProviderConnection connection)
        {
            if (connection.ReloadMode == ReloadMode.ScheduledReload && connection.SecondsToReload <= 0)
                return "Please provided duration at least 1 second.";

            return null;
        }

        private static bool IsValueAnArray(string val)
        {
            val = val.Trim();
            return val.StartsWith("[") && val.EndsWith("]");
        }

        private static bool IsValueObject(string val)
        {
            try
            {
                JsonDocument.Parse(val);
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

    }
}
