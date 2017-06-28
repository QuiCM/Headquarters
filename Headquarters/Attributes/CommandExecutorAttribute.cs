using System;

namespace HQ.Attributes
{
    /// <summary>
    /// Decorates a method to mark it as the executing method for a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandExecutorAttribute : Attribute
    {
        /// <summary>
        /// An enumerable of <see cref="RegexString"/>s that may be used to match input to the command
        /// </summary>
        public RegexString CommandMatcher { get; }
        /// <summary>
        /// A string describing this command
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// A human-readable name for the command
        /// </summary>
        public string FriendlyName { get; }

        /// <summary>
        /// Constructs a new CommandExecutorAttribute using the given <see cref="RegexString"/>s to match input to the command
        /// </summary>
        /// <param name="description">A string describing the command</param>
        /// <param name="commandMatcher">A required RegexString that input must match for the command to be run</param>
        /// <param name="matcherOptions">A RegexStringOptions enum defining how the matcher will behave</param>
        /// <param name="friendlyName">An optional human-readable name for the command</param>
        public CommandExecutorAttribute(string description, string commandMatcher, RegexStringOptions matcherOptions, string friendlyName = null)
        {
            Description = description;
            CommandMatcher = new RegexString(commandMatcher, matcherOptions);
            FriendlyName = friendlyName;
        }
    }
}
