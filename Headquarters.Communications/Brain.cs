using Headquarters.Communications;
using System;

namespace Headquarters.Communications
{
    public class Brain : ContextObject
    {
        public const string BrainChannel = "__NervousSystem";

        public event EventHandler<ContextUpdateEventArgs> OnContextUpdate;

        public void OnContextUpdateReceived(ChannelBase channel, IPublication publication)
        {
            OnContextUpdate?.Invoke(this, new ContextUpdateEventArgs(publication));
        }
    }
}
