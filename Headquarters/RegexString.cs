using System;
using System.Text.RegularExpressions;

namespace HQ
{
    /// <summary>
    /// A String that may or may not also be a <see cref="Regex"/>
    /// </summary>
    public class RegexString
    {
        private Regex _regex;
        private string _string;
        private bool _matchStart;
        private bool _matchEnd;
        private Match _match;
        private bool _matches;

        /// <summary>
        /// Options describing how this RegexString will function
        /// </summary>
        public RegexStringOptions Options { get; set; } = new RegexStringOptions();

        /// <summary>
        /// Constructs a new RegexString with the given string and <see cref="RegexStringOptions"/>
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="options"></param>
        public RegexString(string pattern, RegexStringOptions options)
        {
            Options = options;
            if (!options.PlainText)
            {
                string regexPattern = pattern;
                if (options.EnforceMatchAtStartPosition)
                {
                    //'^' is the regex modifier to assert that the match must begin at the start of the string
                    regexPattern = "^" + regexPattern;
                }
                if (options.EnforceMatchAtEndPosition)
                {
                    //'$' is the regex modifier to assert that the match must end at the end of the string
                    regexPattern = regexPattern + "$";
                }

                _regex = new Regex(regexPattern, !options.CaseSensitive ? RegexOptions.IgnoreCase : RegexOptions.None);
            }

            _matchStart = options.EnforceMatchAtStartPosition;
            _matchEnd = options.EnforceMatchAtEndPosition;

            _string = options.CaseSensitive ? pattern : pattern.ToLowerInvariant();
        }

        /// <summary>
        /// Determines if this RegexString matches the given input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool Matches(string input)
        {
            if (_regex != null)
            {
                return _matches = (_match = _regex.Match(input)).Success;
            }

            if (_matchStart)
            {
                if (_matchEnd)
                {
                    //String must be exactly equal
                    return _matches = Options.CaseSensitive ? input == _string : input.ToLowerInvariant() == _string;
                }

                //String must start with match
                return _matches = Options.CaseSensitive ? input.StartsWith(_string) : input.ToLowerInvariant().StartsWith(_string);
            }

            //String must contain match
            return _matches = Options.CaseSensitive ? input.Contains(_string) : input.ToLowerInvariant().Contains(_string);
        }

        /// <summary>
        /// Removes the matched string from the input.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string RemoveMatchedString(string input)
        {
            if (!_matches)
            {
                return input;
            }

            if (_regex != null)
            {
                if (_match.Success)
                {
                    return input.Remove(0, _match.Length);
                }

                throw new InvalidOperationException($"Matching error - {nameof(RegexString)}.{nameof(Matches)} must be called before removing the matched string.");
            }
            else
            {
                if (_matchStart)
                {
                    if (_matchEnd)
                    {
                        //If we matched start and end, the matched string is the entire string, so return empty
                        return string.Empty;
                    }

                    //If we matched start, remove from the start
                    return input.Remove(0, _string.Length);
                }

                //Remove from anywhere in the string
                int index = Options.CaseSensitive ? input.IndexOf(_string) : input.ToLowerInvariant().IndexOf(_string);
                return input.Remove(index, _string.Length);
            }
        }

        /// <summary>
        /// Provides an implicit conversion from string to <see cref="RegexString"/>.
        /// </summary>
        /// <param name="str"></param>
        public static implicit operator RegexString(string str)
        {
            return new RegexString(str, new RegexStringOptions());
        }

        /// <summary>
        /// Provides an implicit conversion from <see cref="RegexString"/> to string.
        /// RegexStrings converted in this way follow the some conversion as calling <see cref="ToString()"/>
        /// </summary>
        /// <param name="str"></param>
        public static implicit operator string(RegexString str)
        {
            return str._string;
        }

        /// <summary>
        /// Returns the string used to create the RegexString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _string;
        }
    }
}
