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
        /// Adds a '^' character to the beginning of the regex
        /// </summary>
        MatchFromStart = 0,
        /// <summary>
        /// Appends a '$' character to the end of the regex
        /// </summary>
        MatchAtEnd = 1,
        /// <summary>
        /// Creates the RegexString without a regex
        /// </summary>
        PlainText = 2,
        /// <summary>
        /// Enforces case-sensitive matching
        /// </summary>
        CaseSensitive = 4
    }
}
