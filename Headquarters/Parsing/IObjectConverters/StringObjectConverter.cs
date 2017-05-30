using HQ.Interfaces;
using System;

namespace HQ.ObjectConverters
{
    public sealed class StringObjectConverter : IObjectConverter
    {
        public Type ConversionType => typeof(string);

        public object ConvertFromArray<T>(string[] arguments, T context)
        {
            return string.Join(" ", arguments);
        }

        public object ConvertFromString<T>(string argument, T context)
        {
            return argument;
        }
    }
}
