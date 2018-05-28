using HQ.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HQ
{
    /// <summary>
    /// Defines a precondition that must be passed for a command to execute
    /// </summary>
    public class CommandPrecondition
    {
        /// <summary>
        /// Method containing the precondition
        /// </summary>
        public MethodInfo Precondition { get; }

        /// <summary>
        /// Constructs a new CommandPrecondition instance with the given method defining the precondition
        /// </summary>
        /// <param name="pre"></param>
        public CommandPrecondition(MethodInfo pre)
        {
            Precondition = pre;
        }

        /// <summary>
        /// Invokes the precondition
        /// </summary>
        /// <param name="invoker"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public InputResult Invoke(object invoker, IContextObject context)
        {
            if (Precondition == null)
            {
                return InputResult.Unhandled;
            }

            object ret = Precondition.Invoke(invoker, new object[] { context });

            if (ret is bool)
            {
                return ((bool)ret) == true ? InputResult.Success : InputResult.Failure;
            }

            return (InputResult)ret;
        }
    }
}
