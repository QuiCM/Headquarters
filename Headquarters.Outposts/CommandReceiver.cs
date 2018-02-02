using Headquarters.Communications;

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
