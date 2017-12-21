using Headquarters.Communications;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Headquarters.Outposts
{
    /// <summary>
    /// Implements <see cref="IPSProvider"/> to provide pub/sub through Redis
    /// </summary>
    public class RedisConnector : IPSProvider
    {
        private ConnectionMultiplexer _redis;
        private Dictionary<RedisChannel, Action<IChannel, IPublication>> _channelMap;

        /// <summary>
        /// Asynchronously connects to the Redis server described in the given connection string
        /// </summary>
        /// <param name="connectionString">Connection string following the requirements of https://stackexchange.github.io</param>
        /// <returns></returns>
        public async Task ConnectAsync(string connectionString)
        {
            _redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
            _channelMap = new Dictionary<RedisChannel, Action<IChannel, IPublication>>();
        }

        /// <summary>
        /// Publishes a given publication to the given channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="publication"></param>
        /// <returns></returns>
        public async Task PublishAsync(IChannel channel, IPublication publication)
        {
            RedisChannel redisChannel = channel.ToRedisChannel();
            RedisValue message = publication.ToRedisValue();

            StackExchange.Redis.ISubscriber subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(redisChannel, message);
        }

        /// <summary>
        /// Subscribes the service to a given channel, invoking the given callback when messages are received
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task SubscribeAsync(IChannel channel, Action<IChannel, IPublication> callback)
        {
            RedisChannel redisChannel = channel.ToRedisChannel();

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

        private void PublishCallback(RedisChannel channel, RedisValue message)
        {
            if (_channelMap.ContainsKey(channel))
            {
                _channelMap[channel](channel.ToChannel(), message.ToPublication());
            }
        }

        /// <summary>
        /// Disposes the Redis connection
        /// </summary>
        public void Dispose()
        {
            _redis.Dispose();
        }
    }
}
