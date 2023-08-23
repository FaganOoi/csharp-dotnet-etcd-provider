using DotnetEtcdProvider.Exceptions;
using DotnetEtcdProvider.Extensions;
using DotnetEtcdProvider.Models;
using System.Collections.Generic;

namespace DotnetEtcdProvider
{
    public partial class EtcdConfigurationProvider
    {
        private delegate string StringReturnAction();

        private void ValidateConnection(DotnetEtcdProviderConnection connection)
        {
            List<string> errorMessages = new List<string>();
            HandleValidation(() => ValidateUrl(connection.URL), errorMessages);
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

        private string ValidateUrl(string url)
        {
            if (url.HasData() && url.IsUrlWithProxy())
                return null;
            return "Please provided valid connection to access Etcd";
        }

        private string ValidateAuth(string username, string password)
        {
            if (username.IsEmpty() && password.IsEmpty())
                return null;
            if (username.HasData() && password.HasData())
            {
                _etcdRequiredAuth = true;
                return null;
            }

            return "Please check on username and password provided";
        }

        private string ValidateReloadMode(DotnetEtcdProviderConnection connection)
        {
            if (connection.ReloadMode == ReloadMode.ScheduledReload && connection.SecondsToReload <= 0)
                return "Please provided duration at least 1 second.";

            if (connection.ReloadMode == ReloadMode.OnChangeReload
                && (connection.PrefixListUsedToWatch is null || connection.PrefixListUsedToWatch.Count <= 0)
            )
                return "Please provided at least 1 prefix to watch.";

            return null;
        }

        private static bool IsValueAnArray(string val)
        {
            return val.StartsWith("[") && val.EndsWith("]");
        }

        private static bool IsValueObject(string val)
        {
            return val.StartsWith("{") && val.EndsWith("}");
        }

    }
}
