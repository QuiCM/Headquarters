using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Headquarters.Communications
{
    /// <summary>
    /// Describes a publisher in the Pub/Sub model. Used to publish publications to channels
    /// </summary>
    public interface IPublisher
    {
        /// <summary>
        /// Asynchronously publishes a given publication to a given channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="publication"></param>
        /// <returns></returns>
        Task PublishAsync(ChannelBase channel, IPublication publication);
    }
}
