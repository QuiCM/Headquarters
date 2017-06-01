namespace HQ
{
    /// <summary>
    /// Delegate for a method called when an input is processed and its result returned
    /// </summary>
    /// <param name="result">The result of the processing</param>
    /// <param name="output">The output from the processing.</param>
    public delegate void InputResultDelegate(InputResult result, object output);
}
