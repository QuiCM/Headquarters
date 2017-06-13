using HQ.Interfaces;
using System;

namespace HQ.Parsing.IObjectConverters
{
    /// <summary>
    /// Converts a string or a string[] to a string[]
    /// </summary>
    public sealed class StringArrayObjectConverter : IObjectConverter
    {
        /// <inheritdoc/>
        public Type ConversionType => typeof(string[]);

        /// <inheritdoc/>
        public object ConvertFromArray<T>(string[] arguments, T context)
        {
            return arguments;
        }

        /// <inheritdoc/>
        public object ConvertFromString<T>(string argument, T context)
        {
            return argument.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
