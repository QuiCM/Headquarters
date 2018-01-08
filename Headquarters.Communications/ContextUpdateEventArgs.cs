namespace Headquarters.Communications
{
    public class ContextUpdateEventArgs
    {
        public IPublication Content { get; }

        public ContextUpdateEventArgs(IPublication content)
        {
            Content = content;
        }
    }
}