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
            string errorMsg = "";
            HandleValidation(() => ValidateUrl(connection.URL), ref errorMsg);
            HandleValidation(() => ValidateAuth(connection.Username, connection.Password), ref errorMsg);
            HandleValidation(() => ValidateReloadMode(connection.SecondsToReload), ref errorMsg);

            if (errorMsg.HasData())
                throw new EtcdProviderConfigurationException(errorMsg, connection);
        }

        private void HandleValidation(StringReturnAction action, ref String msg)
        {
            string result = action.Invoke();
            if (result.HasData())
            {
                if (msg.IsEmpty())
                    msg = result;
                else
                    msg = $"{msg} - {result}";
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
    }
}
