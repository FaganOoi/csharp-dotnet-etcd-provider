namespace DotnetEtcdProvider.Model
{
    public class DotnetEtcdProviderConnection
    {
        public string URL { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int SecondsToReload { get; set; } = 5;
    }
}
