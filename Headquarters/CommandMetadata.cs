using HQ.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HQ.Extensions;

namespace HQ
{
    /// <summary>
    /// Describes the metadata captured during command parsing
    /// </summary>
    public class CommandMetadata
    {
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
        /// The command's Type
        /// </summary>
        public Type Type { get; internal set; }
        /// <summary>
        /// A message describing this command
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Aliases the command can be executed with
        /// </summary>
        public IEnumerable<RegexString> Aliases { get; internal set; }
        /// <summary>
        /// Metadata for each subcommand defined on this command
        /// </summary>
        public IEnumerable<CommandMetadata> SubcommandMetadata { get; internal set; }
        /// <summary>
        /// Whether or not this command has subcommands
        /// </summary>
        public bool HasSubcommands => SubcommandMetadata != null && SubcommandMetadata.Count() > 0;
    }
}
