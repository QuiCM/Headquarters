using System;

namespace HQ.Attributes
{
    /// <summary>
    /// Decorates a class to mark it as containing commands.
    /// Allows definition of the caching rule for this command
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandClassAttribute : Attribute
    {
        /// <summary>
        /// Decorates a class containing a command executor
        /// </summary>
        public CommandClassAttribute()
        {
        }
    }
}
