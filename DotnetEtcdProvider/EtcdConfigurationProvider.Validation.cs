using DotnetEtcdProvider.Exceptions;
using DotnetEtcdProvider.Extensions;
using DotnetEtcdProvider.Model;

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
            HandleValidation(() => ValidateAutoReloadDuration(connection.SecondsToReload), ref errorMsg);

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

        private string ValidateAutoReloadDuration(int duration)
        {
            if (duration >= 0)
                return null;

            return "Please provided duration at least zero.";
        }
    }
}
