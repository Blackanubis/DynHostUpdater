using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using DynHostUpdater.Helpers;
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

        private DateTime? _lastCheck;

        private string _login;
        private string _password;

        private string _publicIP;
        private string _hostIP;
        private int _timeToRefresh;
        private string _urlUpdater;

        #endregion

        #region Ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusViewModel"/> class.
        /// </summary>
        public StatusViewModel()
        {
            SaveCommand = new AsyncRelayCommand<object>(DoSaveAsync, obj => !IsBusy && IsDirty);
            CancelCommand = new AsyncRelayCommand(DoCancelAsync, () => !IsBusy && IsDirty);

            Task.Run(InitAsync);
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
            set
            {
                if (!Set(ref _isDirty, value)) return;

                SaveCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
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
        public DateTime? LastCheck
        {
            get => _lastCheck;
            set => Set(ref _lastCheck, value);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Loads the data.
        /// </summary>
        private async Task InitAsync()
        {
            try
            {
                IsBusy = true;

                await LoadDataAsync().ContinueWith(task => StartOrRestartTask());
            }
            catch (Exception e)
            {
                WriteConsole(e.Message, null);
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

                if (!string.IsNullOrWhiteSpace(Password))
                    App.Configuration.Password = Password;

                await App.SaveJsonFile().ConfigureAwait(false);

                await StartOrRestartTask();
            }
            catch (Exception e)
            {
                WriteConsole(e.Message, null);
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
        private async Task LoadDataAsync()
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
        private Task StartOrRestartTask()
        {
            if (_autoRefreshCancellationTokenSource != null &&
                !_autoRefreshCancellationTokenSource.IsCancellationRequested)
            {
                _autoRefreshCancellationTokenSource.Cancel();
            }

            _autoRefreshCancellationTokenSource = new CancellationTokenSource();

            _ = CheckIPMatchAsync(_autoRefreshCancellationTokenSource.Token);

            return Task.CompletedTask;
        }


        /// <summary>
        /// Refreshes the public ip.
        /// </summary>
        /// <param name="token">The token.</param>
        public async Task CheckIPMatchAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Get IP public and host
                    var getPublicIPTask = GetPublicIP(token);
                    var getHostIPTask = GetHostIP(HostAdress, token);

                    var publicIP = await getPublicIPTask;
                    var hostIP = await getHostIPTask;

                    if (!token.IsCancellationRequested)
                    {
                        PublicIP = publicIP;
                        HostIP = hostIP;

                        // update DynHost if different
                        if (!string.IsNullOrWhiteSpace(UrlUpdater) &&
                            HostIP != PublicIP &&
                            await UpdateDynHost(UrlUpdater, PublicIP, Login, Password, token))
                        {
                            WriteConsole($"{HostIP} updated to {PublicIP}", token);
                            HostIP = PublicIP;
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteConsole(e.Message, token);
                }

                LastCheck = DateTime.Now;

                if (!token.IsCancellationRequested)
                    await Task.Delay(TimeSpan.FromSeconds(TimeToRefresh), token);
            }
        }

        /// <summary>
        /// Gets the public ip.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static async Task<string> GetPublicIP(CancellationToken token)
        {
            var ip = string.Empty;

            try
            {
                using var webClient = new WebClient();
                ip = await webClient.DownloadStringTaskAsync("http://icanhazip.com");
            }
            catch (Exception e)
            {
                WriteConsole(e.Message, token);
            }

            return ip.Trim();
        }

        /// <summary>
        /// Gets the host ip.
        /// </summary>
        /// <param name="hostAdress">The host urlUpdater.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static async Task<string> GetHostIP(string hostAdress, CancellationToken token)
        {
            var ip = string.Empty;

            try
            {
                var ipAddress = await Dns.GetHostAddressesAsync(hostAdress);
                ip = ipAddress.Select(address => address.ToString()).FirstOrDefault();
            }
            catch (Exception e)
            {
                WriteConsole(e.Message, token);
            }

            return ip;
        }

        /// <summary>
        ///     Updates the dyn host.
        /// </summary>
        /// <param name="urlUpdater">The urlUpdater.</param>
        /// <param name="ip">The ip.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task<bool> UpdateDynHost(string urlUpdater, string ip, string username, string password, CancellationToken token)
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
                        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{EncryptionHelper.Decrypt(password)}")));

                using var client = new HttpClient
                {
                    DefaultRequestHeaders = { Authorization = authValue }
                };

                // make full urlUpdater
                var uriAdress = string.Format(urlUpdater, ip);
                if (!uriAdress.StartsWith("http://"))
                    uriAdress = string.Concat("http://", uriAdress);

                if (!token.IsCancellationRequested)
                {
                    // Get the response.
                    var response = await client.GetAsync(uriAdress, token);

                    if (response.StatusCode == HttpStatusCode.OK)
                        return true;

                    WriteConsole(response.StatusCode.ToString(), token);
                }
            }
            catch (Exception e)
            {
                WriteConsole(e.Message, token);
            }

            return await Task.FromResult(false);
        }

        /// <summary>
        /// Writes the console.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="token">The token.</param>
        private static void WriteConsole(string message, CancellationToken? token)
        {
            if (token == null | !token.GetValueOrDefault().IsCancellationRequested)
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
        public AsyncRelayCommand<object> SaveCommand { get; }
        /// <summary>
        /// Does the save asynchronous.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private async Task DoSaveAsync(object parameter)
        {
            if (parameter is PasswordBox passwordBox)
                Password = EncryptionHelper.Encrypt(passwordBox.Password);

            await SaveDataAsync();
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
            await LoadDataAsync();
        }

        #endregion
    }
}