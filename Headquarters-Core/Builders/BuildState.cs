using System;
using System.Collections.Generic;
using System.Text;

namespace Headquarters_Core.Builders
{
    public enum BuildStatus
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
    /// Contains information about the state of an IBuilder
    /// </summary>
    public class BuildState<TResult>
    {
        /// <summary>
        /// Event invoked when a build's status changes
        /// </summary>
        public event EventHandler<(BuildStatus status, object data)> OnStatusChange;

        /// <summary>
        /// Result of the build operation
        /// </summary>
        public BuildStatus Result { get; set; } = BuildStatus.Pending;
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
            Result = BuildStatus.Pending;
            Exception = null;
        }

        /// <summary>
        /// Sets the BuildState to <see cref="BuildStatus.Faulted"/> and records the given exception
        /// </summary>
        /// <param name="e"></param>
        public TResult Fail(Exception e)
        {
            Result = BuildStatus.Faulted;
            Exception = e;

            OnStatusChange?.Invoke(this, (Result, e));

            return default;
        }

        /// <summary>
        /// Adds a warning to the list of warnings and sets build state to <see cref="BuildStatus.Warning"/>
        /// </summary>
        /// <param name="message"></param>
        public void Warn(string message)
        {
            Result &= BuildStatus.Warning;
            _warnings.Add(message);

            OnStatusChange?.Invoke(this, (Result, _warnings));
        }

        /// <summary>
        /// Sets the BuildState to <see cref="BuildStatus.Success"/>
        /// </summary>
        public TResult Succeed(TResult result)
        {
            Result = BuildStatus.Success;
            OnStatusChange?.Invoke(this, (Result, null));

            return result;
        }
    }
}
