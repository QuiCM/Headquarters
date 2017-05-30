namespace HQ
{
    /// <summary>
    /// Describes the result of an input after processing
    /// </summary>
    public enum InputResult
    {
        /// <summary>
        /// The input was not recognized as a command, as was ignored. The Result object will be null
        /// </summary>
        Unhandled,
        /// <summary>
        /// The input was succesfully handled as a command. The Result object will contain the command output
        /// </summary>
        Success,
        /// <summary>
        /// The input failed to be handled as a command. The Result object will contain an exception
        /// </summary>
        Failure
    }
}
