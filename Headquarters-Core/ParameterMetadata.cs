using System;
using System.Reflection;

namespace Headquarters_Core
{
    /// <summary>
    /// Contains information about a command's parameter
    /// </summary>
    public class ParameterMetadata
    {
        /// <summary>
        /// Reflected ParameterInfo type data
        /// </summary>
        public ParameterInfo ParameterInfo { get; set; }
        /// <summary>
        /// Parameter attribute if any is present
        /// </summary>
        public HQCommandParameterAttribute Attribute { get; set; }
        /// <summary>
        /// Uses <see cref="HQCommandParameterAttribute.Name"/> property or <see cref="System.Reflection.ParameterInfo.Name"/> 
        /// if no parameter attribute was present
        /// </summary>
        public string Name => Attribute?.Name ?? ParameterInfo.Name;
        /// <summary>
        /// Uses <see cref="HQCommandParameterAttribute.Required"/> property, or false if no attribute is present
        /// </summary>
        public bool Required => Attribute?.Required ?? false;
        /// <summary>
        /// True if <see cref="HQCommandParameterAttribute.CustomMatcher"/> is present
        /// </summary>
        public bool HasCustomMatcher => Attribute?.CustomMatcher != null;
        /// <summary>
        /// Type describing the ParameterBuilder used to build the parameter
        /// </summary>
        public Type ParameterBuilder => Attribute?.ParameterBuilder;
    }
}