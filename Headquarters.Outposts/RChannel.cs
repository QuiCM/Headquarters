using Headquarters.Communications;
using StackExchange.Redis;
using System;
using static StackExchange.Redis.RedisChannel;

namespace Headquarters.Outposts
{
    /// <summary>
    /// Implements an <see cref="IChannel"/> for Redis by wrapping <see cref="RedisChannel"/>
    /// </summary>
    public struct RChannel : IChannel, IEquatable<RChannel>
    {
        private RedisChannel _channel;

        /// <summary>
        /// Creates a new RChannel from the given <see cref="RedisChannel"/>
        /// </summary>
        /// <param name="channel"></param>
        public RChannel(RedisChannel channel)
        {
            _channel = channel;
        }

        /// <summary>
        /// Creates a new RChannel. Wraps <see cref="RedisChannel.RedisChannel(byte[], PatternMode)"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        public RChannel(byte[] value, PatternMode mode)
        {
            _channel = new RedisChannel(value, mode);
        }

        /// <summary>
        /// Creates a new RChannel. Wraps <see cref="RedisChannel.RedisChannel(string, PatternMode)"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        public RChannel(string value, PatternMode mode)
        {
            _channel = new RedisChannel(value, mode);
        }

        /// <summary>
        /// Determines equality of this RChannel with another object. Wraps <see cref="RedisChannel.Equals(object)"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return _channel.Equals(obj);
        }

        /// <summary>
        /// Determines equality of this RChannel with another RChannel. Wraps <see cref="RedisChannel.Equals(RedisChannel)"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(RChannel other)
        {
            return _channel.Equals(other._channel);
        }

        /// <summary>
        /// Returns the hashcode of this RChannel. Wraps <see cref="RedisChannel.GetHashCode()"/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _channel.GetHashCode();
        }

        /// <summary>
        /// Determines equality of two RChannels. Wraps <see cref="RedisChannel.operator =="/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(RChannel left, RChannel right)
        {
            return left._channel == right._channel;
        }

        /// <summary>
        /// Determines inequality of two RChannels. Wraps <see cref="RedisChannel.operator !="/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(RChannel left, RChannel right)
        {
            return !(left == right);
        }

        public static implicit operator RChannel(RedisChannel channel)
        {
            return new RChannel(channel);
        }

        public static implicit operator RedisChannel(RChannel channel)
        {
            return channel._channel;
        }
    }
}
