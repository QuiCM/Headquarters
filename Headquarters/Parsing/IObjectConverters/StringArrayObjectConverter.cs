using HQ.Interfaces;
using System;

namespace HQ.Parsing.IObjectConverters
{
    public sealed class StringArrayObjectConverter : IObjectConverter
    {
        public Type ConversionType => typeof(string[]);

        public object ConvertFromArray<T>(string[] arguments, T context)
        {
            return arguments;
        }

        public object ConvertFromString<T>(string argument, T context)
        {
            return argument.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
