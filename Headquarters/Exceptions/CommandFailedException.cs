using System;

namespace HQ.Exceptions
{
    /// <summary>
    /// Exception thrown when command execution fails.
    /// </summary>
    public class CommandFailedException : Exception
    {
        public CommandFailedException(string message, Exception inner = null) : base(message, inner) { }
    }
}
