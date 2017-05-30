using System;

namespace HQ.Interfaces
{
    /// <summary>
    /// Responsible from converting a string or string[] to a specific object type
    /// </summary>
    public interface IObjectConverter
    {
        /// <summary>
        /// The type that this converter will convert to
        /// </summary>
        Type ConversionType { get; }

        /// <summary>
        /// Converts a string array to an object
        /// </summary>
        /// <returns></returns>
        object ConvertFromArray<T>(string[] arguments, T context);

        /// <summary>
        /// Converts a single string to an object
        /// </summary>
        /// <returns></returns>
        object ConvertFromString<T>(string argument, T context);
    }
}
