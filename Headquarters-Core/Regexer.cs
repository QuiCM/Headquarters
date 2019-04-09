using System.Text;
using System.Text.RegularExpressions;

namespace Headquarters_Core
{
    public class Regexer
    {
        /// <summary>
        /// Matches "command name {param} {param} {..param}" with all whitespace optional
        /// </summary>
        private readonly Regex _commandFormatRegex = new Regex(@"(?<command>[\w\s]+)\b(?>\s*)(?>(?<params>{[\w]+\??})(?>\s*)*)*", RegexOptions.Compiled);

        public const string ParamMatcher =         "\"[^\"]*\"|[^\\s\"]*";
        public const string RequiredParamMatcher = "\"[^\"]+\"|[^\\s\"]+";
        public const string ParameterDefaultReplacer = "{default}";
        private const string _whitespace = "(?>\\s*)";

        public Regex GenerateCommandMatcher(CommandMetadata metadatum)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"(?<command>{metadatum.Name})")
              .Append($"{_whitespace}");

            foreach (ParameterMetadata param in metadatum.Parameters)
            {
                string matcher;
                if (param.HasCustomMatcher)
                {
                    matcher = param.Attribute.CustomMatcher.Replace(
                        ParameterDefaultReplacer, 
                        param.Required ? 
                            $"(?<{param.ParameterInfo.Name}>{RequiredParamMatcher})" : 
                            $"(?<{param.ParameterInfo.Name}>{ParamMatcher})"
                    );
                }
                else
                {
                    matcher = param.Required ? RequiredParamMatcher : ParamMatcher;
                }

                //Must use ParameterInfo.Name for group name, as user defined names may not work with the regex spec
                sb.Append($@"(?<{param.ParameterInfo.Name}>{matcher})")
                  .Append(_whitespace);
            }

            return new Regex(sb.ToString(), RegexOptions.Compiled);
        }
    }
}
