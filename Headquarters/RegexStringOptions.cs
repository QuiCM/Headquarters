using System;

namespace HQ
{
    /// <summary>
    /// Options to classify how a <see cref="RegexString"/> behaves
    /// </summary>
    [Flags]
    public enum RegexStringOptions
    {
        /// <summary>
        /// No options will be included
        /// </summary>
        None = 0,
        /// <summary>
        /// Adds a '^' character to the beginning of the regex
        /// </summary>
        MatchFromStart = 1,
        /// <summary>
        /// Appends a '$' character to the end of the regex
        /// </summary>
        MatchAtEnd = 2,
        /// <summary>
        /// Creates the RegexString without a regex
        /// </summary>
        PlainText = 4,
        /// <summary>
        /// Enforces case-sensitive matching
        /// </summary>
        CaseSensitive = 8
    }
}
