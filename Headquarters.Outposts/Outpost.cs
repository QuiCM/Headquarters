using Headquarters.Communications;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Headquarters.Outposts
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Outpost : IDisposable
    {
        private PubSubProviderBase _pubSubProvider;
        private CommandReceiver _cmdReceiver;
        private Configuration _config;
        private string _connectionString;
        private Brain _brain;
        private bool _loadedCustomProvider = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public Outpost(string connectionString) => _connectionString = connectionString;

        internal void ReadConfig(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                _config = ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap
                    {
                        ExeConfigFilename = path
                    },
                    ConfigurationUserLevel.None
               );
            }
            else
            {
                _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }

            _connectionString = _config.ConnectionStrings.ConnectionStrings["PubSubProvider"].ConnectionString;
        }

        internal void SetProvider(string provider)
        {
            if (File.Exists(provider))
            {
                //Load provider from external path
                Assembly asm = Assembly.LoadFrom(provider);
                _pubSubProvider = (PubSubProviderBase)Activator.CreateInstance(
                    asm.GetExportedTypes().First(t => t.GetInterfaces().Contains(typeof(PubSubProviderBase))),
                    new object[] { new Brain() }
                );

                _loadedCustomProvider = true;
            }
            else
            {
                Console.WriteLine("Invalid provider file specified.");
                Environment.Exit(4041);
            }
        }

        /// <summary>
        /// Attempts an asynchronous connection with the configured connection string.
        /// This is a blocking call and is considered the entry point to an Outpost
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            Console.WriteLine("Awaiting connection to Redis server...");
            
            if (!_loadedCustomProvider)
            {
                _pubSubProvider = new RedisConnector(new Brain());
            }

            await _pubSubProvider.ConnectAsync(_connectionString)
                .ContinueWith((t) => Console.WriteLine("Connection established."))
                .ContinueWith((t) => Console.Write("Subscribing to queues..."))
                .ContinueWith(async (t) =>
                {
                    //Subscribe to a queue for context forwarding
                    await _pubSubProvider.SubscribeAsync(ChannelFactory.CreateFromString(Brain.BrainChannel, _pubSubProvider.ChannelType), _pubSubProvider.Brain.OnContextUpdateReceived);
                    //Subscribe to a queue for each loaded command
                    await _pubSubProvider.SubscribeAsync(null, _cmdReceiver.OnReceive);
                })
                .ContinueWith((t) => Console.WriteLine("\tComplete."));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _pubSubProvider.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _config = null;
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Outpost() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
