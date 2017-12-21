using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Headquarters.Communications
{
    /// <summary>
    /// Describes a Pub/Sub service provider. Used to pump stateful changes between Headquarters and Outposts
    /// </summary>
    public interface IPSProvider : IPublisher, ISubscriber, IDisposable
    {
        /// <summary>
        /// Asynchronously connect to the pub/sub service with the given connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        Task ConnectAsync(string connectionString);
    }
}
