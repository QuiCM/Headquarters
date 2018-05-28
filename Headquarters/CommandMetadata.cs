using System;
using System.Collections.Generic;
using System.Linq;

namespace HQ
{
    /// <summary>
    /// Describes the metadata captured during command parsing
    /// </summary>
    public class CommandMetadata
    {
        /// <summary>
        /// The command's Type
        /// </summary>
        public Type Type { get; internal set; }
        /// <summary>
        /// Executing methods present on the command's type
        /// </summary>
        public IEnumerable<CommandExecutorData> Executors { get; internal set; }
        /// <summary>
        /// Precondition method present on the command's type
        /// </summary>
        public CommandPrecondition Precondition { get; internal set; }

        /// <summary>
        /// Returns all executors that match the given input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IEnumerable<CommandExecutorData> GetExecutorData(string input)
        {
            return Executors?.Where(e => e.ExecutorAttribute.CommandMatcher.Matches(input));
        }

        /// <summary>
        /// Returns the first executor that matches the given input, or a default value if none was found
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public CommandExecutorData GetFirstOrDefaultExecutorData(string input)
        {
            return Executors?.FirstOrDefault(e => e.ExecutorAttribute.CommandMatcher.Matches(input));
        }
    }
}
