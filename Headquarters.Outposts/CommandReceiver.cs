using Headquarters.Communications;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headquarters.Outposts
{
    public sealed class CommandReceiver
    {

        public void OnReceive(ChannelBase channel, IPublication publication)
        {
            //Command = Commands.First(c => c.Name == channel)
        }
    }
}
