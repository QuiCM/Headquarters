using HQ.Interfaces;
using System;
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
        /// <summary>
        /// The type of parser that will be used when parsing input and executing commands
        /// </summary>
        public Type Parser { get; set; } = typeof(Parsing.Parser);
        /// <summary>
        /// The string or character used to define the pipe between two commands
        /// </summary>
        public string PipeCharacter = "|";
    }
}
