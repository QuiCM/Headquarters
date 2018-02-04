using Headquarters.Communications;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Headquarters.Outposts
{
    /// <summary>
    /// Implements <see cref="PubSubProvider"/> to provide pub/sub through Redis
    /// </summary>
    public class RedisConnector : PubSubProvider
    {
        private ConnectionMultiplexer _redis;
        private Dictionary<StackExchange.Redis.RedisChannel, Func<ChannelBase, IPublication, Task>> _channelMap;

        public override Type ChannelType => typeof(RedisChannel);

        public RedisConnector(Brain brain) : base(brain) { }

        /// <summary>
        /// Asynchronously connects to the Redis server described in the given connection string
        /// </summary>
        /// <param name="connectionString">Connection string following the requirements of https://stackexchange.github.io</param>
        /// <returns></returns>
        public override async Task ConnectAsync(string connectionString)
        {
            _redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
            _channelMap = new Dictionary<StackExchange.Redis.RedisChannel, Func<ChannelBase, IPublication, Task>>();
        }

        /// <summary>
        /// Publishes a given publication to the given channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="publication"></param>
        /// <returns></returns>
        public override async Task PublishAsync(ChannelBase channel, IPublication publication)
        {
            StackExchange.Redis.RedisChannel redisChannel = (RedisChannel)channel;
            RedisValue message = (RedisPublication)publication;

            StackExchange.Redis.ISubscriber subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(redisChannel, message);
        }

        /// <summary>
        /// Subscribes the service to a given channel, invoking the given callback when messages are received
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public override async Task SubscribeAsync(ChannelBase channel, Func<ChannelBase, IPublication, Task> callback)
        {
            StackExchange.Redis.RedisChannel redisChannel = (RedisChannel)channel;

            if (_channelMap.ContainsKey(redisChannel))
            {
                _channelMap[redisChannel] = callback;
            }
            else
            {
                _channelMap.Add(redisChannel, callback);
            }

            StackExchange.Redis.ISubscriber subscriber = _redis.GetSubscriber();
            await subscriber.SubscribeAsync(redisChannel, PublishCallback);
        }

        private void PublishCallback(StackExchange.Redis.RedisChannel channel, RedisValue message)
        {
            if (_channelMap.ContainsKey(channel))
            {
                _channelMap[channel]((Headquarters.Outposts.RedisChannel)channel, (Headquarters.Outposts.RedisPublication)message).Wait();
            }
        }

        /// <summary>
        /// Disposes the Redis connection
        /// </summary>
        public override void Dispose()
        {
            _redis.Dispose();
        }
    }
}
