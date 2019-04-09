using System;
using System.Collections.Generic;
using System.Text;

namespace Headquarters_Core
{
    /// <summary>
    /// Decorates a parameter in a HQ Command parameter, providing extra metadata
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.GenericParameter)]
    public class HQCommandParameterAttribute : Attribute
    {
        /// <summary>
        /// User-friendly parameter name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Whether or not this parameter must be present
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// Custom regex expression used to match the parameter.
        /// Occurences of <see cref="Regexer.ParameterDefaultReplacer"/> will be replaced with 
        /// <see cref="Regexer.ParamMatcher"/> or <see cref="Regexer.RequiredParamMatcher"/> if <see cref="Required"/> is true
        /// </summary>
        public string CustomMatcher { get; set; }
        /// <summary>
        /// Type used to construct the parameter. Must inherit <see cref="Headquarters_Core.ParameterBuilder"/>
        /// </summary>
        public Type ParameterBuilder { get; set; }
    }
}
