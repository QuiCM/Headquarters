using System;

namespace Headquarters.Communications
{
    public static class ChannelFactory
    {
        public static ChannelBase CreateFromString(string channel, Type type)
        {
            if (type.IsAbstract)
            {
                throw new ArgumentException("Channel type may not be abstract.", nameof(type));
            }

            if (type.IsInterface)
            {
                throw new ArgumentException("Channel type may not be an interface.", nameof(type));
            }
            
            if (!type.IsSubclassOf(typeof(ChannelBase)))
            {
                throw new ArgumentException("Channel type must be a subclass of " + nameof(ChannelBase), nameof(type));
            }

            return ((ChannelBase)Activator.CreateInstance(type)).FromString(channel);
        }
    }
}
