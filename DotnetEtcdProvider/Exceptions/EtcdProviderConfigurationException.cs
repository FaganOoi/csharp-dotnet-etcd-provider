using DotnetEtcdProvider.Models;
using System;

namespace DotnetEtcdProvider.Exceptions
{
    public class EtcdProviderConfigurationException : Exception
    {
        private readonly string _message;
        private readonly DotnetEtcdProviderConnection _connection;

        public EtcdProviderConfigurationException(string message, DotnetEtcdProviderConnection connection)
        {
            _message = message;
            _connection = connection;
        }
    }
}
