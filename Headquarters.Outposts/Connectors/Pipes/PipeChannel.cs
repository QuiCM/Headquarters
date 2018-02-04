using Headquarters.Communications;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headquarters.Outposts.Connectors.Pipes
{
    /// <summary>
    /// Implements <see cref="ChannelBase"/> to provide a channel object for use by named pipes
    /// </summary>
    public class PipeChannel : ChannelBase
    {
        private string _channel;

        /// <summary>
        /// Constructs a <see cref="PipeChannel"/> object from the given string
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public PipeChannel(string name)
        {
            _channel = name;
        }

        public static implicit operator PipeChannel(string str)
        {
            return new PipeChannel(str);
        }

        /// <summary>
        /// Constructs a <see cref="PipeChannel"/> object from the given string.
        /// For use with <see cref="ChannelFactory.CreateFromString(string, Type)"/>
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public override ChannelBase FromString(string channel)
        {
            return new PipeChannel(channel);
        }
    }
}
