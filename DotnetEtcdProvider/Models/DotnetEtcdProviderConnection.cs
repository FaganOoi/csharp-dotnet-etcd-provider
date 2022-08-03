using System.Collections.Generic;

namespace DotnetEtcdProvider.Models
{
    public class DotnetEtcdProviderConnection
    {
        /// <summary>
        /// Connection string included http/https
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Username for authentication
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password for authentication
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Duration to reload data when `ReloadMode` is ScheduledReload
        /// </summary>
        public int SecondsToReload { get; set; }

        /// <summary>
        /// List of prefix will be watch  when `ReloadMode` is OnChangeReload
        /// </summary>
        public List<string> PrefixListUsedToWatch { get; set; }

        /// <summary>
        /// Mode to configure how to update data from time to time
        /// </summary>
        public ReloadMode ReloadMode { get; set; } = ReloadMode.OnChangeReload;
    }
}
