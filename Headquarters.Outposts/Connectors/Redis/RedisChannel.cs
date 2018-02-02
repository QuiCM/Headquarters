using Headquarters.Communications;
using System;
using static StackExchange.Redis.RedisChannel;

namespace Headquarters.Outposts
{
    /// <summary>
    /// Implements a <see cref="ChannelBase"/> for Redis by wrapping <see cref="StackExchange.Redis.RedisChannel"/>
    /// </summary>
    public class RedisChannel : ChannelBase, IEquatable<RedisChannel>
    {
        private StackExchange.Redis.RedisChannel _channel;

        /// <summary>
        /// Creates a new RedisChannel from the given <see cref="StackExchange.Redis.RedisChannel"/>
        /// </summary>
        /// <param name="channel"></param>
        public RedisChannel(StackExchange.Redis.RedisChannel channel)
        {
            _channel = channel;
        }

        /// <summary>
        /// Creates a new RedisChannel. Wraps <see cref="StackExchange.Redis.RedisChannel.RedisChannel(byte[], PatternMode)"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        public RedisChannel(byte[] value, PatternMode mode)
        {
            _channel = new StackExchange.Redis.RedisChannel(value, mode);
        }

        /// <summary>
        /// Creates a new RedisChannel. Wraps <see cref="StackExchange.Redis.RedisChannel.RedisChannel(string, PatternMode)"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        public RedisChannel(string value, PatternMode mode)
        {
            _channel = new StackExchange.Redis.RedisChannel(value, mode);
        }

        public override ChannelBase FromString(string channel)
        {
            _channel = new StackExchange.Redis.RedisChannel(channel, PatternMode.Auto);
            return this;
        }

        /// <summary>
        /// Determines equality of this RedisChannel with another object. Wraps <see cref="StackExchange.Redis.RedisChannel.Equals(object)"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return _channel.Equals(obj);
        }

        /// <summary>
        /// Determines equality of this RedisChannel with another RedisChannel. Wraps <see cref="StackExchange.Redis.RedisChannel.Equals(StackExchange.Redis.RedisChannel)"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(RedisChannel other)
        {
            return _channel.Equals(other._channel);
        }

        /// <summary>
        /// Returns the hashcode of this RedisChannel. Wraps <see cref="StackExchange.Redis.RedisChannel.GetHashCode()"/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _channel.GetHashCode();
        }

        /// <summary>
        /// Determines equality of two RedisChannels. Wraps <see cref="StackExchange.Redis.RedisChannel.operator =="/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(RedisChannel left, RedisChannel right)
        {
            return left._channel == right._channel;
        }

        /// <summary>
        /// Determines inequality of two RedisChannels. Wraps <see cref="StackExchange.Redis.RedisChannel.operator !="/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(RedisChannel left, RedisChannel right)
        {
            return !(left == right);
        }

        public static implicit operator RedisChannel(StackExchange.Redis.RedisChannel channel)
        {
            return new RedisChannel(channel);
        }

        public static implicit operator StackExchange.Redis.RedisChannel(RedisChannel channel)
        {
            return channel._channel;
        }
    }
}
