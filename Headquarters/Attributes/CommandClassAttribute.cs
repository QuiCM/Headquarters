using System;

namespace HQ.Attributes
{
    /// <summary>
    /// Decorates a class to mark it as containing commands.
    /// Allows definition of the caching rule for this command
    /// </summary>
    public class CommandClassAttribute : Attribute
    {
        /// <summary>
        /// Whether or not the command should be executed asynchronously
        /// </summary>
        public bool AsyncExecution { get; set; }

        /// <summary>
        /// Decorates a class containing commands, and optionally tells the parser to execute the command asynchronously
        /// </summary>
        /// <param name="async">Whether or not the command should be executed asynchronously</param>
        public CommandClassAttribute(bool async = false)
        {
            AsyncExecution = async;
        }
    }
}
