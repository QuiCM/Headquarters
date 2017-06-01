using System;

namespace HQ.Attributes
{
    /// <summary>
    /// Decorates a method to mark it as the executing method for a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandExecutorAttribute : Attribute
    {
    }
}
