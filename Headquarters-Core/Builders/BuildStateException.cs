using System;
using System.Collections.Generic;
using System.Text;

namespace Headquarters_Core.Builders
{
    [Serializable]
    public class BuildStateException : Exception
    {
        public Type FailedType { get; }

        public BuildStateException(string message, Type failedType) : base(message)
        {
            FailedType = failedType;
        }

        public BuildStateException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
