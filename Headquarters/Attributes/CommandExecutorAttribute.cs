﻿using System;

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
        /// A short description of the command
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// A human-readable name for the command
        /// </summary>
        public string FriendlyName => _friendlyName ?? CommandMatcher?.ToString();
        /// <summary>
        /// A long description of the command
        /// </summary>
        public string LongDescription
        {
            set { _longDescription = value; }
            get { return _longDescription ?? Description; }
        }

        private string _longDescription;
        private string _friendlyName;

        /// <summary>
        /// Constructs a new CommandExecutorAttribute using the given <see cref="RegexString"/>s to match input to the command
        /// </summary>
        /// <param name="description">A short string describing the command</param>
        /// <param name="commandMatcher">A required RegexString that input must match for the command to be run</param>
        /// <param name="matcherOptions">A RegexStringOptions enum defining how the matcher will behave</param>
        /// <param name="friendlyName">An optional human-readable name for the command</param>
        /// <param name="longDescription">A long string describing the command</param>
        public CommandExecutorAttribute(string description, string commandMatcher, RegexStringOptions matcherOptions, string friendlyName = null,
            string longDescription = null)
        {
            Description = description;
            CommandMatcher = new RegexString(commandMatcher, matcherOptions);
            _friendlyName = friendlyName;
            _longDescription = longDescription;
        }
    }
}
