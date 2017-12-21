using Headquarters.Communications;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace Headquarters.Outposts
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Outpost : IDisposable
    {
        private IPSProvider _pubSubProvider;
        private Configuration _config;
        private string _connectionString;

        internal Outpost(string connectionString) => _connectionString = connectionString;

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

            _connectionString = _config.ConnectionStrings.ConnectionStrings["Redis"].ConnectionString;
        }

        /// <summary>
        /// Attempts an asynchronous connection with the configured connection string.
        /// This is a blocking call and is considered the entry point to an Outpost
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            Console.WriteLine("Awaiting connection to Redis server...");

            await _pubSubProvider.ConnectAsync(_connectionString)
                .ContinueWith((t) => Console.WriteLine("Connection established."));
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
