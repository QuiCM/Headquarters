using Headquarters.Communications;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headquarters.Outposts.Connectors.Pipes
{
    public class PipeChannel : ChannelBase
    {
        private string _channel;

        public PipeChannel(string name)
        {
            _channel = name;
        }

        public override ChannelBase FromString(string channel)
        {
            return new PipeChannel(channel);
        }
    }
}
