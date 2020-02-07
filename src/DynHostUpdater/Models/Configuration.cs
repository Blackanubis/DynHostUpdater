namespace DynHostUpdater.Models
{
    /// <summary>
    /// Configuration
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets or sets the time to refresh.
        /// </summary>
        /// <value>
        /// The time to refresh.
        /// </value>
        public int TimeToRefresh { get; set; } = 60;
        /// <summary>
        /// Gets or sets the host adress.
        /// </summary>
        /// <value>
        /// The host adress.
        /// </value>
        public string HostAdress { get; set; }
        /// <summary>
        /// Gets or sets the URL updater.
        /// </summary>
        /// <value>
        /// The URL updater.
        /// </value>
        public string UrlUpdater { get; set; }
        /// <summary>
        /// Gets or sets the login.
        /// </summary>
        /// <value>
        /// The login.
        /// </value>
        public string Login { get; set; }
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }
    }
}
