using System;
using System.Collections.Generic;
using System.Text;

namespace HQ.Attributes
{
    /// <summary>
    /// Decorates a method to mark it as the error handler for a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ErrorHandlerAttribute : Attribute
    {
    }
}
