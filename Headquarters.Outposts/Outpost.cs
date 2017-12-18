using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace Headquarters.Outpost
{
    public sealed class Outpost
    {
        private ConnectionMultiplexer _redis;
        private Configuration _config;
        private string _connectionString;

        /// <summary>
        /// 
        /// </summary>
        public ConnectionMultiplexer Redis => _redis;

        internal Outpost(string configPath) => ReadConfig(configPath);

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
        /// Attempts an asynchronous connection with the configured connection string
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            Console.WriteLine("Awaiting connection to Redis server...");
            _redis = await ConnectionMultiplexer.ConnectAsync(_connectionString)
                .ContinueWith((connection) =>
                {
                    Console.WriteLine("Connection established.");
                    return connection.Result;
                }
           );
        }
    }
}
