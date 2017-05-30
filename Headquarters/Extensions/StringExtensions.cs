using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HQ.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Explodes a string of input into a list of strings
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> Explode(this string input)
        {
            return ObjectiveExplode(input).Cast<string>().ToList();
        }

        /// <summary>
        /// Explodes a string of input into a list of objects
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<object> ObjectiveExplode(this string input)
        {
            List<object> exploded = new List<object>();

            if (string.IsNullOrWhiteSpace(input))
            {
                return exploded;
            }

            bool openedQuote = false;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '"')
                {
                    if (openedQuote)
                    {
                        exploded.Add(sb.ToString());
                        sb.Clear();
                        openedQuote = false;
                    }
                    else
                    {
                        openedQuote = true;
                    }
                    continue;
                }

                if (input[i] == ' ' && !openedQuote)
                {
                    if (sb.Length > 0)
                    {
                        exploded.Add(sb.ToString());
                    }
                    sb.Clear();
                }
                else
                {
                    sb.Append(input[i]);
                }
            }

            if (sb.Length > 0)
            {
                exploded.Add(sb.ToString());
            }

            exploded.RemoveAll(e => string.IsNullOrWhiteSpace(e.ToString()));

            return exploded;
        }
    }
}
