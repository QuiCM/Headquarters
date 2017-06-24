using HQ.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HQ
{
    /// <summary>
    /// Contains information about an executing method present on a command
    /// </summary>
    public class CommandExecutorData
    {
        /// <summary>
        /// Whether or not the command should be run asynchronously
        /// </summary>
        public bool AsyncExecution { get; internal set; }
        /// <summary>
        /// The Attribute decorating the method. For subcommands this will be a <see cref="SubcommandExecutorAttribute"/>.
        /// For base commands this will be a <see cref="CommandExecutorAttribute"/>
        /// </summary>
        public CommandExecutorAttribute ExecutorAttribute { get; internal set; }
        /// <summary>
        /// The method that will be called when the command is executed
        /// </summary>
        public MethodInfo ExecutingMethod { get; internal set; }
        /// <summary>
        /// The number of arguments that are required for the ExecutingMethod to be executed
        /// </summary>
        public int RequiredArguments { get; internal set; }
        /// <summary>
        /// Key-value pairs describing each parameter of the executing method of the command
        /// </summary>
        public Dictionary<ParameterInfo, CommandParameterAttribute> ParameterData { get; internal set; }
        /// <summary>
        /// Parent command of this command. Null if this is not a subcommand
        /// </summary>
        public CommandExecutorData ParentCommand { get; internal set; }
        /// <summary>
        /// Subcommands of the command, null if none exist
        /// </summary>
        public IEnumerable<CommandExecutorData> Subcommands { get; internal set; }
        /// <summary>
        /// Whether or not the command has subcommands
        /// </summary>
        public bool HasSubcommands => Subcommands?.Count() > 0;
    }
}
