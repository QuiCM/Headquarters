using HQ.Interfaces;
using System;

namespace HQ.Parsing.IObjectConverters
{
    /// <summary>
    /// Converts a string or string[] into an int
    /// </summary>
    public sealed class IntObjectConverter : IObjectConverter
    {
        /// <inheritdoc/>
        public Type ConversionType => typeof(int);

        /// <inheritdoc/>
        public object ConvertFromArray<T>(string[] arguments, T context)
        {
            if (!int.TryParse(string.Join(" ", arguments), out int res))
            {
                return null;
            }
            return res;
        }

        /// <inheritdoc/>
        public object ConvertFromString<T>(string argument, T context)
        {
            if (!int.TryParse(argument, out int res))
            {
                return null;
            }
            return res;
        }
    }
}
