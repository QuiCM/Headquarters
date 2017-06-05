using HQ.Interfaces;
using System.Collections.Generic;

namespace HQ
{
    /// <summary>
    /// Settings that affect the way a <see cref="CommandRegistry"/> functions
    /// </summary>
    public class RegistrySettings
    {
        /// <summary>
        /// Whether or not the default converters in <see cref="HQ.Parsing.IObjectConverters"/> should be added by default
        /// </summary>
        public bool EnableDefaultConverters { get; set; } = false;
        /// <summary>
        /// Enumerable of <see cref="IObjectConverter"/>s that will be registered for conversions
        /// </summary>
        public IEnumerable<IObjectConverter> Converters { get; set; }
    }
}
