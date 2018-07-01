using HQ.Interfaces;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace HQ
{
    /// <summary>
    /// Defines a handler that captures error data when a command is executed
    /// </summary>
    public class CommandErrorHandler
    {
        /// <summary>
        /// Method containing the handler
        /// </summary>
        public MethodInfo Callback { get; }
        /// <summary>
        /// Whether the handler method should be executed asynchronously or not
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        /// Constructs a new CommandErrorHandler instance with the given method defining the handler
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="isAsync"></param>
        public CommandErrorHandler(MethodInfo handler, bool isAsync)
        {
            Callback = handler;
            IsAsync = isAsync;
        }

        /// <summary>
        /// Invokes the handler
        /// </summary>
        /// <param name="invoker"></param>
        /// <param name="context"></param>
        /// <param name="expectedType"></param>
        /// <param name="givenValue"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public void Invoke(object invoker, IContextObject context, Type expectedType, string givenValue, Exception ex)
        {
            if (Callback == null)
            {
                return;
            }

            Callback.Invoke(invoker, new object[] { context, expectedType, givenValue, ex });
        }

        /// <summary>
        /// Asynchronously invokes the handler
        /// </summary>
        /// <param name="invoker"></param>
        /// <param name="context"></param>
        /// <param name="expectedType"></param>
        /// <param name="givenValue"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public async Task InvokeAsync(object invoker, IContextObject context, Type expectedType, string givenValue, Exception ex)
        {
            if (Callback == null)
            {
                return;
            }

            await (Task)Callback.Invoke(invoker, new object[] { context, expectedType, givenValue, ex });
        }
    }
}
