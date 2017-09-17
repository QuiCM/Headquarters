using HQ.Attributes;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HQ
{
    /// <summary>
    /// A Regex matching string designed for parsing command triggers
    /// </summary>
    public class RegexString
    {
        private static readonly Regex FormatRegex = new Regex(@"{(?<format>[\w]+\??)}");

        private Regex _regex;
        private string _string;
        private Match _match;
        private bool _matches;

        /// <summary>
        /// Options describing how this RegexString will function
        /// </summary>
        public RegexStringOptions Options { get; set; }
        /// <summary>
        /// Format parameter groups present in the regex string
        /// </summary>
        public List<string> FormatParameters { get; private set; } = new List<string>();
        /// <summary>
        /// Links a format parameter group name to the length of its captured string
        /// </summary>
        public Dictionary<string, CommandParameterAttribute> FormatData { get; private set; } = new Dictionary<string, CommandParameterAttribute>();

        /// <summary>
        /// Constructs a new RegexString with the given string and <see cref="RegexStringOptions"/>
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="options"></param>
        public RegexString(string pattern, RegexStringOptions options)
        {
            Options = options;
            string regexPattern = pattern;
            if (options.HasFlag(RegexStringOptions.MatchFromStart))
            {
                //'^' is the regex modifier to assert that the match must begin at the start of the string
                regexPattern = "^" + regexPattern;
            }
            if (options.HasFlag(RegexStringOptions.MatchAtEnd))
            {
                //'$' is the regex modifier to assert that the match must end at the end of the string
                regexPattern = regexPattern + "$";
            }

            regexPattern = FormatRegex.Replace(regexPattern, (match) =>
            {
                string arg = match.Groups["format"].Value;
                bool optional = false;

                //a ? makes the format param optional
                if (arg.EndsWith("?") || arg.StartsWith("?"))
                {
                    arg = arg.Replace("?", "");
                    optional = true;
                }

                FormatParameters.Add(arg);
                FormatData.Add(arg, new CommandParameterAttribute(repetitions: 0, optional: optional));

                //("[^"]*"|[^"]+) => matches "First word" more words "third and more words" in 3 groups
                return $"(?<{arg}>\"[^\"]*\"|[^\"]+){(optional ? "?" : "")}";
            });

            _regex = new Regex(regexPattern, !options.HasFlag(RegexStringOptions.CaseSensitive) ? RegexOptions.IgnoreCase : RegexOptions.None);
            _string = regexPattern;
        }

        /// <summary>
        /// Determines if this RegexString matches the given input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool Matches(string input)
        {
            return _matches = (_match = _regex.Match(input)).Success;
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
            if (_match.Success)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (string group in _regex.GetGroupNames())
                {
                    if (group == "0")
                    {
                        //'0' is the entire match, and not an explicitly defined named match, so we skip it
                        continue;
                    }
                    string value = _match.Groups[group].Value;

                    //Each matched group should be added to the parameters required for parsing
                    sb.Append(value).Append(" ");
                    //And format data should be populated with the number of strings captured, for dynamically sizing format parameters
                    if (value.StartsWith("\""))
                    {
                        //The regex splits quoted messages separately. I.e., "word \"word 2\" word3" -> "word", "word 2", "word3".
                        //Quoted messages should only count for 1 repetition.
                        FormatData[group].Repetitions = 1;
                    }
                    else
                    {
                        FormatData[group].Repetitions = value.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length;
                    }
                }
                sb.Append(input.Remove(0, _match.Length));

                return sb.ToString();
            }

            throw new InvalidOperationException($"Matching error - {nameof(RegexString)}.{nameof(Matches)} must succeed before removing the matched string.");
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
