namespace Headquarters.Communications
{
    public abstract class ChannelBase
    {
        protected ChannelBase() { }

        public abstract ChannelBase FromString(string channel);
    }
}
