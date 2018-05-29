using HQ.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        /// Whether the precondition method should be executed asynchronously or not
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        /// Constructs a new CommandPrecondition instance with the given method defining the precondition
        /// </summary>
        /// <param name="pre"></param>
        /// <param name="isAsync"></param>
        public CommandPrecondition(MethodInfo pre, bool isAsync)
        {
            Precondition = pre;
            IsAsync = isAsync;
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

            if (ret is bool bRet)
            {
                return bRet == true ? InputResult.Success : InputResult.Failure;
            }

            return (InputResult)ret;
        }

        /// <summary>
        /// Asynchronously invokes the precondition
        /// </summary>
        /// <param name="invoker"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<InputResult> InvokeAsync(object invoker, IContextObject context)
        {
            if (Precondition == null)
            {
                return InputResult.Unhandled;
            }

            object ret = Precondition.Invoke(invoker, new object[] { context });

            if (ret is Task<bool> taskBool)
            {
                return await taskBool ? InputResult.Success : InputResult.Failure;
            }

            return await (ret as Task<InputResult>);
        }
    }
}
