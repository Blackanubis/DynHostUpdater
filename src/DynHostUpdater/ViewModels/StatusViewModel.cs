using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyToolkit.Command;
using MyToolkit.Mvvm;

namespace DynHostUpdater.ViewModels
{
    /// <summary>
    ///     StatusViewModel
    /// </summary>
    /// <seealso cref="ViewModelBase" />
    public class StatusViewModel : ViewModelBase
    {
        #region Fields

        private CancellationTokenSource _autoRefreshCancellationTokenSource;

        private string _hostAdress;

        private bool _isBusy;
        private bool _isDirty;

        private DateTime _lastCheck;

        private string _login;
        private string _password;

        private string _publicIP;
        private string _hostIP;
        private int _timeToRefresh;
        private string _urlUpdater;

        #endregion

        #region Ctors

        public StatusViewModel()
        {
            CancelCommand = new AsyncRelayCommand(DoCancelAsync);
            SaveCommand = new AsyncRelayCommand(DoSaveAsync);
            LoadDataAsync();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a value indicating whether this instance is busy.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is busy; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusy
        {
            get => _isBusy;
            private set => Set(ref _isBusy, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is dirty.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is dirty; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirty
        {
            get => _isDirty;
            set => Set(ref _isDirty, value);
        }

        /// <summary>
        ///     Gets the ip address extended network.
        /// </summary>
        /// <value>
        ///     The ip address extended network.
        /// </value>
        public string PublicIP
        {
            get => _publicIP;
            set => Set(ref _publicIP, value);
        }

        /// <summary>
        ///     Gets the site ip.
        /// </summary>
        /// <value>
        ///     The site ip.
        /// </value>
        public string HostIP
        {
            get => _hostIP;
            set => Set(ref _hostIP, value);
        }

        /// <summary>
        ///     Gets the time to refresh.
        /// </summary>
        /// <value>
        ///     The time to refresh.
        /// </value>
        public int TimeToRefresh
        {
            get => _timeToRefresh;
            set
            {
                if (Set(ref _timeToRefresh, value)) IsDirty = true;
            }
        }

        /// <summary>
        ///     Gets the host urlUpdater.
        /// </summary>
        /// <value>
        ///     The host urlUpdater.
        /// </value>
        public string HostAdress
        {
            get => _hostAdress;
            set
            {
                if (Set(ref _hostAdress, value)) IsDirty = true;
            }
        }

        /// <summary>
        ///     Gets the URL updater.
        /// </summary>
        /// <value>
        ///     The URL updater.
        /// </value>
        public string UrlUpdater
        {
            get => _urlUpdater;
            set
            {
                if (Set(ref _urlUpdater, value)) IsDirty = true;
            }
        }

        /// <summary>
        ///     Gets or sets the login.
        /// </summary>
        /// <value>
        ///     The login.
        /// </value>
        public string Login
        {
            get => _login;
            set
            {
                if (Set(ref _login, value)) IsDirty = true;
            }
        }

        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        /// <value>
        ///     The password.
        /// </value>
        public string Password
        {
            get => _password;
            set
            {
                if (Set(ref _password, value)) IsDirty = true;
            }
        }

        /// <summary>
        ///     Gets or sets the last check.
        /// </summary>
        /// <value>
        ///     The last check.
        /// </value>
        public DateTime LastCheck
        {
            get => _lastCheck;
            set => Set(ref _lastCheck, value);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Loads the data.
        /// </summary>
        private async void LoadDataAsync()
        {
            try
            {
                IsBusy = true;

                TimeToRefresh = App.Configuration.TimeToRefresh;

                HostAdress = App.Configuration.HostAdress;

                UrlUpdater = App.Configuration.UrlUpdater;

                Login = App.Configuration.Login;

                Password = App.Configuration.Password;

                await StartOrRestartTaskAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                WriteConsole(e.Message);
                throw;
            }
            finally
            {
                IsDirty = false;
                IsBusy = false;
            }
        }

        /// <summary>
        ///     Saves the data.
        /// </summary>
        private async Task SaveDataAsync()
        {
            try
            {
                IsBusy = true;

                App.Configuration.TimeToRefresh = TimeToRefresh;
                App.Configuration.HostAdress = HostAdress;
                App.Configuration.UrlUpdater = UrlUpdater;
                App.Configuration.Login = Login;
                App.Configuration.Password = Password;

                await App.SaveJsonFile().ConfigureAwait(false);

                await StartOrRestartTaskAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                WriteConsole(e.Message);
                throw;
            }
            finally
            {
                IsDirty = false;
                IsBusy = false;
            }
        }

        /// <summary>
        ///     Cancels the data.
        /// </summary>
        private async Task CancelDataAsync()
        {
            TimeToRefresh = App.Configuration.TimeToRefresh;
            HostAdress = App.Configuration.HostAdress;
            UrlUpdater = App.Configuration.UrlUpdater;
            Login = App.Configuration.Login;
            Password = App.Configuration.Password;

            IsDirty = false;
            IsBusy = false;

            await Task.CompletedTask;
        }

        /// <summary>
        ///     Starts the or restart task.
        /// </summary>
        public async Task StartOrRestartTaskAsync()
        {
            if (_autoRefreshCancellationTokenSource != null &&
                !_autoRefreshCancellationTokenSource.IsCancellationRequested)
                _autoRefreshCancellationTokenSource.Cancel();

            _autoRefreshCancellationTokenSource = new CancellationTokenSource();
            await CheckIPMatchAsync(_autoRefreshCancellationTokenSource).ConfigureAwait(true);
        }

        /// <summary>
        ///     Refreshes the public ip.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        public async Task CheckIPMatchAsync(CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    // Get public IP of workstation
                    PublicIP = await GetPublicIP().ConfigureAwait(true);

                    // Get IP of Domaines
                    HostIP = await GetHostIP(HostAdress).ConfigureAwait(true);

                    // update DynHost if different
                    if (!string.IsNullOrWhiteSpace(UrlUpdater) &&
                        HostIP != PublicIP &&
                        await UpdateDynHost(UrlUpdater, PublicIP, Login, Password))
                    {
                        WriteConsole($"{HostIP} updated to {PublicIP}");
                        HostIP = PublicIP;
                    }
                }
                catch (Exception e)
                {
                    WriteConsole(e.Message);
                }

                LastCheck = DateTime.Now;

                await Task.Delay(TimeSpan.FromSeconds(TimeToRefresh)).ConfigureAwait(true);
            }
        }

        /// <summary>
        ///     Gets the public ip.
        /// </summary>
        /// <returns></returns>
        private static async Task<string> GetPublicIP()
        {
            return await Task.FromResult(new WebClient().DownloadString("http://icanhazip.com").Trim());
        }

        /// <summary>
        ///     Gets the host ip.
        /// </summary>
        /// <param name="hostAdress">The host urlUpdater.</param>
        /// <returns></returns>
        private static async Task<string> GetHostIP(string hostAdress)
        {
            var ipaddress = Dns.GetHostAddresses(hostAdress);
            return await Task.FromResult(ipaddress.Select(ip => ip.ToString()).FirstOrDefault()).ConfigureAwait(true);
        }

        /// <summary>
        ///     Updates the dyn host.
        /// </summary>
        /// <param name="urlUpdater">The urlUpdater.</param>
        /// <param name="ip">The ip.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private static async Task<bool> UpdateDynHost(string urlUpdater, string ip, string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(urlUpdater))
                    throw new ArgumentException(nameof(urlUpdater));

                if (string.IsNullOrEmpty(ip))
                    throw new ArgumentException(nameof(ip));

                AuthenticationHeaderValue authValue = null;

                if (!string.IsNullOrEmpty(ip) || !string.IsNullOrEmpty(ip))
                    authValue = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

                var client = new HttpClient
                {
                    DefaultRequestHeaders = {Authorization = authValue}
                };

                // make full urlUpdater
                var fullAdress = string.Format(urlUpdater, ip);

                // Get the response.
                var response = await client.GetAsync(string.Format(fullAdress, ip));

                if (response.StatusCode == HttpStatusCode.OK)
                    return true;
                WriteConsole(response.StatusCode.ToString());
            }
            catch (Exception e)
            {
                WriteConsole(e.Message);
            }

            return await Task.FromResult(false).ConfigureAwait(true);
        }

        /// <summary>
        ///     Writes the console.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void WriteConsole(string message)
        {
            Console.WriteLine($@"{DateTime.Now:G} : {message}");
        }

        #endregion

        #region Command

        /// <summary>
        /// Gets the save command.
        /// </summary>
        /// <value>
        /// The save command.
        /// </value>
        public AsyncRelayCommand SaveCommand { get; }
        /// <summary>
        ///     Does the save asynchronous.
        /// </summary>
        /// <returns></returns>
        private async Task DoSaveAsync()
        {
            await SaveDataAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        /// <value>
        /// The cancel command.
        /// </value>
        public AsyncRelayCommand CancelCommand { get; }

        /// <summary>
        ///     Does the cancel asynchronous.
        /// </summary>
        /// <returns></returns>
        private async Task DoCancelAsync()
        {
            await CancelDataAsync().ConfigureAwait(true);
        }

        #endregion
    }
}