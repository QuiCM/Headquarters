using System;
using System.Collections.Generic;
using System.Text;

namespace HQ.Attributes
{
    /// <summary>
    /// Decorates a method to mark it as a precondition that must be satisfied before the command is run
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PreconditionAttribute : Attribute
    {
    }
}
