using System;
using System.Collections.Generic;
using System.Text;

namespace Headquarters_Core.Builders
{
    /// <summary>
    /// Contains information about the state of an IBuilder
    /// </summary>
    public class BuildState<TResult>
    {
        public enum Status
        {
            /// <summary>
            /// The build has not yet completed
            /// </summary>
            Pending = 0,
            /// <summary>
            /// The build completed without issue
            /// </summary>
            Success = 1,
            /// <summary>
            /// The build completed with warnings
            /// </summary>
            Warning = 2,
            /// <summary>
            /// The build has faulted
            /// </summary>
            Faulted = 4
        }

        /// <summary>
        /// Result of the build operation
        /// </summary>
        public Status Result { get; set; } = Status.Pending;
        /// <summary>
        /// Exception caught in the build
        /// </summary>
        public Exception Exception { get; private set; }
        /// <summary>
        /// Warnings captured during build
        /// </summary>
        public IEnumerable<string> Warnings => _warnings;

        private List<string> _warnings = new List<string>();

        /// <summary>
        /// Clears build state
        /// </summary>
        public void Reset()
        {
            _warnings.Clear();
            Result = Status.Pending;
            Exception = null;
        }

        /// <summary>
        /// Sets the BuildState to <see cref="Status.Faulted"/> and records the given exception
        /// </summary>
        /// <param name="e"></param>
        public TResult Fail(Exception e)
        {
            Result = Status.Faulted;
            Exception = e;

            return default;
        }

        /// <summary>
        /// Adds a warning to the list of warnings and sets build state to <see cref="Status.Warning"/>
        /// </summary>
        /// <param name="message"></param>
        public void Warn(string message)
        {
            Result |= Status.Warning;
            _warnings.Add(message);
        }

        /// <summary>
        /// Sets the BuildState to <see cref="Status.Success"/>
        /// </summary>
        public TResult Succeed(TResult result)
        {
            Result = Status.Success;
            return result;
        }
    }
}
