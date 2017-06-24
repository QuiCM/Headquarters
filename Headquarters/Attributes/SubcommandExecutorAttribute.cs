using System;
using System.Collections.Generic;
using System.Linq;

namespace HQ.Attributes
{
    /// <summary>
    /// Decorates a method to mark it as a sub-executor for a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SubcommandExecutorAttribute : CommandExecutorAttribute
    {
        /// <summary>
        /// A string representing the name of the command that executes this subcommand
        /// </summary>
        public string ParentName { get; }

        /// <summary>
        /// Constructs a new SubcommandExecutorAttribute using the given <see cref="RegexString"/>s to match input to the command
        /// </summary>
        /// <param name="parentCommand">A string representing the name of the command that executes this subcommand</param>
        /// <param name="description">A string describing the command</param>
        /// <param name="commandMatcher">A required RegexString that input must match for the command to be run</param>
        /// <param name="commandMatchers">Optional RegexStrings that input may match for the command to be run</param>
        public SubcommandExecutorAttribute(string parentCommand, string description, string commandMatcher, params string[] commandMatchers)
            : base(description, commandMatcher, commandMatchers)
        {
            ParentName = parentCommand;
        }
    }
}
