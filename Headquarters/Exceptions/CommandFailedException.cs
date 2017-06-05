using System;

namespace HQ.Exceptions
{
    /// <summary>
    /// Exception thrown when command execution fails.
    /// </summary>
    public class CommandFailedException : Exception
    {
        /// <summary>
        /// Generates a new CommandFailedException with the given message and optional inner exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public CommandFailedException(string message, Exception inner = null) : base(message, inner) { }
    }
}
