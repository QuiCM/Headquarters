using Headquarters.Communications;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headquarters.Outposts
{
    public static class IPSExtensions
    {
        /// <summary>
        /// Converts an IChannel into a RedisChannel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static RedisChannel ToRedisChannel(this IChannel channel) => channel.ToString();
        /// <summary>
        /// Converts an <see cref="IPublication"/> into a <see cref="RedisValue"/>
        /// </summary>
        /// <param name="publication"></param>
        /// <returns></returns>
        public static RedisValue ToRedisValue(this IPublication publication) => publication.ToString();
        
        /// <summary>
        /// Converts a <see cref="RedisChannel"/> into an <see cref="IChannel"/>
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static IChannel ToChannel(this RedisChannel channel)
        {
            return default;
        }

        /// <summary>
        /// Converts a <see cref="RedisValue"/> into an <see cref="IPublication"/>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IPublication ToPublication(this RedisValue message)
        {
            return default;
        }
    }
}
