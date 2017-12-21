﻿using System;
using System.Threading.Tasks;

namespace Headquarters.Communications
{
    /// <summary>
    /// Describes a subscriber in the Pub/Sub model. Used to subscribe to channels
    /// </summary>
    public interface ISubscriber
    {
        /// <summary>
        /// Subscribe to the given channel, performing the given callback when publications are received
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="callback"></param>
        Task SubscribeAsync(IChannel channel, Action<IChannel, IPublication> callback);
    }
}
