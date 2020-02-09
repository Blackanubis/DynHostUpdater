using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DynHostUpdater.Helpers;
using DynHostUpdater.Models;
using Newtonsoft.Json;

namespace DynHostUpdater
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region Fields

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new JsonPrivateSetterContractResolver()
            };

        private static readonly string _jsonFilePath = "Settings.json";

        private static readonly object Locker = new object();

        #endregion

        #region Ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            MainApp().Wait();
        }

        /// <summary>
        /// Mains the application.
        /// </summary>
        private static async Task MainApp()
        {
            try
            {
                Configuration = await LoadOrCreateJsonFileAsync();
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the configuration.
        /// </summary>
        /// <value>
        ///     The configuration.
        /// </value>
        public static Configuration Configuration { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Loads the or create json file.
        /// </summary>
        /// <returns></returns>
        private static async Task<Configuration> LoadOrCreateJsonFileAsync()
        {
            var f = new FileInfo(_jsonFilePath);

            var configuration = !f.Exists
                ? new Configuration()
                : Deserialize(File.ReadAllText(_jsonFilePath));

            return await Task.FromResult(configuration);
        }

        /// <summary>
        ///   Commits this instance.
        /// </summary>
        /// <returns></returns>
        public static async Task SaveJsonFile()
        {
            WriteToFile(Serialize(Configuration));

            await Task.CompletedTask;
        }

        /// <summary>
        ///   Writes to file.
        /// </summary>
        /// <param name="text">The text.</param>
        private static void WriteToFile(string text)
        {
            lock (Locker)
            {
                new FileInfo(_jsonFilePath).Delete();

                var file = new FileStream(
                    _jsonFilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read
                );

                using var writer = new StreamWriter(file, Encoding.Unicode);

                writer.Write(text);

                writer.Dispose();
                file.Dispose();
            }
        }

        /// <summary>
        ///     Serializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static string Serialize(Configuration context)
        {
            return JsonConvert.SerializeObject(context, JsonSerializerSettings);
        }

        /// <summary>
        ///     Deserializes the specified json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static Configuration Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<Configuration>(
                json,
                JsonSerializerSettings
            );
        }

        #endregion
    }
}