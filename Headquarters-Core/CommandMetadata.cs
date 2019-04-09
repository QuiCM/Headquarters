using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Headquarters_Core
{
    /// <summary>
    /// Contains information about a command
    /// </summary>
    public class CommandMetadata
    {
        /// <summary>
        /// Command's name. Defined by <see cref="HQCommandAttribute.Name"/> or <see cref="MemberInfo.Name"/> if no name was specified in the attribute
        /// </summary>
        public string Name => Attribute.Name ?? Method.Name;
        /// <summary>
        /// Attribute associated with the command
        /// </summary>
        public HQCommandAttribute Attribute { get; set; }
        /// <summary>
        /// Method representing the command logic
        /// </summary>
        public MethodInfo Method { get; set; }
        /// <summary>
        /// Parameter information defined by the command method
        /// </summary>
        public IEnumerable<ParameterMetadata> Parameters { get; set; }
        /// <summary>
        /// Regex used to match text to the command
        /// </summary>
        public Regex Matcher { get; set; }
    }
}
