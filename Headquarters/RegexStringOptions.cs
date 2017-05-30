namespace HQ
{
    /// <summary>
    /// Options to classify how a <see cref="RegexString"/> behaves
    /// </summary>
    public class RegexStringOptions
    {
        /// <summary>
        /// If true, a '^' character is added to the beginning of the regex,
        /// meaning the match must begin at the start of the input string
        /// </summary>
        public bool EnforceMatchAtStartPosition = false;
        /// <summary>
        /// If true, a '$' character is added to the end of the regex,
        /// meaning the match must end at the end of the input string
        /// </summary>
        public bool EnforceMatchAtEndPosition = false;
        /// <summary>
        /// If true, this RegexString will not be matched with a regex,
        /// but will use string comparisons instead
        /// </summary>
        public bool PlainText = false;
        /// <summary>
        /// If true, this RegexString will match case-sensitively.
        /// </summary>
        public bool CaseSensitive = false;

        /// <summary>
        /// Sets the value of <see cref="PlainText"/>
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public RegexStringOptions SetPlainText(bool plainText = true)
        {
            PlainText = plainText;
            return this;
        }

        /// <summary>
        /// Sets the value of <see cref="EnforceMatchAtStartPosition"/>
        /// </summary>
        /// <param name="matchAtStart"></param>
        /// <returns></returns>
        public RegexStringOptions SetMatchAtStart(bool matchAtStart = true)
        {
            EnforceMatchAtStartPosition = matchAtStart;
            return this;
        }

        /// <summary>
        /// Sets the value of <see cref="EnforceMatchAtEndPosition"/>
        /// </summary>
        /// <param name="matchAtEnd"></param>
        /// <returns></returns>
        public RegexStringOptions SetMatchAtEnd(bool matchAtEnd = true)
        {
            EnforceMatchAtEndPosition = matchAtEnd;
            return this;
        }

        /// <summary>
        /// Sets the value of <see cref="CaseSensitive"/>
        /// </summary>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        public RegexStringOptions SetCaseSensitivity(bool caseSensitive = true)
        {
            CaseSensitive = caseSensitive;
            return this;
        }

        /// <summary>
        /// Shortcut to <see cref="RegexStringOptions.SetPlainText(bool)"/>
        /// </summary>
        public static RegexStringOptions PlainTextOptions = new RegexStringOptions() { PlainText = true };
        /// <summary>
        /// Shortcut to <see cref="RegexStringOptions.SetMatchAtStart(bool)"/>
        /// </summary>
        public static RegexStringOptions MatchStartOptions = new RegexStringOptions() { EnforceMatchAtStartPosition = true };
        /// <summary>
        /// Shortcut to <see cref="RegexStringOptions.SetMatchAtEnd(bool)"/>
        /// </summary>
        public static RegexStringOptions MatchEndOptions = new RegexStringOptions() { EnforceMatchAtEndPosition = true };
    }
}
