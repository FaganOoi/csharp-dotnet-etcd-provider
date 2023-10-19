using System.Collections.Generic;

namespace DotnetEtcdProvider.Models
{
    public class DotnetEtcdProviderConnection
    {
        /// <summary>
        /// Connection string included http/https
        /// We will prioritise on single endpoint
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Connections string included http/https
        /// </summary>
        public List<string> URLs { get; set; }

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
        /// It is a list of prefix that will get data from ETCD and look on the changes based on reload mode.
        /// If not data provided, we will assume get all data and look on changes of "/"
        /// </summary>
        public List<string> PrefixData { get; set; } = new List<string>();

        /// <summary>
        /// List of prefix will to remove when keep into provider
        /// </summary>
        public List<string> PrefixListUsedToRemoveInData { get; set; }

        /// <summary>
        /// Mode to configure how to update data from time to time or display continue watching
        /// </summary>
        public ReloadMode ReloadMode { get; set; } = ReloadMode.OnChangeReload;
    }
}
