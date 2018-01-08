using System;
using System.Threading.Tasks;

namespace Headquarters.Communications
{
    /// <summary>
    /// Describes a Pub/Sub service provider. Used to pump stateful changes between Headquarters and Outposts
    /// </summary>
    public abstract class PubSubProviderBase : IPublisher, ISubscriber, IDisposable
    {
        public abstract Type ChannelType { get; }
        public Brain Brain { get; }

        public PubSubProviderBase(Brain brain)
        {
            Brain = brain;
        }

        /// <summary>
        /// Asynchronously connect to the pub/sub service with the given connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public abstract Task ConnectAsync(string connectionString);

        public abstract Task PublishAsync(ChannelBase channel, IPublication publication);

        public abstract Task SubscribeAsync(ChannelBase channel, Action<ChannelBase, IPublication> callback);

        public abstract void Dispose();
    }
}
