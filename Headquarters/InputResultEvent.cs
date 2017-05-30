using System;

namespace HQ
{
    /// <summary>
    /// Data provided when an input finishes processing
    /// </summary>
    public class InputResultEventArgs : EventArgs
    {
        /// <summary>
        /// The ID of the input handled
        /// </summary>
        public int ID { get; }
        /// <summary>
        /// The result of the input after handling
        /// </summary>
        public InputResult Result { get; }
        /// <summary>
        /// The output from any command invoked by the input
        /// </summary>
        public object Output { get; }

        /// <summary>
        /// Constructs a new InputResultEvent with the given result and output
        /// </summary>
        /// <param name="result"></param>
        /// <param name="output"></param>
        public InputResultEventArgs(InputResult result, object output, int id)
        {
            Result = result;
            Output = output;
            ID = id;
        }
    }
}
