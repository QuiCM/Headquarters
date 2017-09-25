namespace HQ.Interfaces
{
    /// <summary>
    /// An object that describes the context of a command
    /// </summary>
    public interface IContextObject : IConcurrentStorage<object>
    {
        /// <summary>
        /// A reference to the CommandRegistry relevant to this context
        /// </summary>
        CommandRegistry Registry { get; }
        /// <summary>
        /// Whether or not this context should be reused, or flagged as deleted
        /// </summary>
        bool Finalized { get; set; }
    }
}
