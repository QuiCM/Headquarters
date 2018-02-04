using Headquarters.Communications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Headquarters.Outposts.Connectors.Pipes
{
    /// <summary>
    /// Implements <see cref="IPublication"/> to provide a publication object for use by named pipes.
    /// Publication format follows the Redis Pub/Sub design:
    /// Message = *messageLength\r\n
    ///           :Number\r\n
    ///           +Simple string\r\n
    ///           $complexLength\r\nComplex\r\nstring with multiple\r\nline breaks\r\n
    ///           -Error\r\n
    /// </summary>
    public class PipePublication : IPublication
    {
        /// <summary>
        /// Number of strings in the publication enumerable
        /// </summary>
        public int Length => Message.Count();

        /// <summary>
        /// Contents of the publication enumerable
        /// </summary>
        public IEnumerable<string> Message { get; }

        /// <summary>
        /// Constructs a new <see cref="PipePublication"/> by converting the given objects to recognized types or strings
        /// </summary>
        /// <param name="messages"></param>
        public PipePublication(params object[] messages)
        {
            List<string> message = new List<string>
            {
                "*" + messages.Length
            };

            foreach (object obj in messages)
            {
                if (obj is null)
                {
                    message.Add("$-1\r\n");
                    continue;
                }

                if (obj is int)
                {
                    //Pub/Sub int = :Num
                    message.Add(":" + obj);
                    continue;
                }

                if (obj is Exception e)
                {
                    message.Add("-" + e.Message);
                    continue;
                }

                if (obj is string str && !str.Contains("\r\n"))
                {
                    //Pub/sub simple string = +Str
                    message.Add("+" + obj);
                    continue;
                }

                string complex = obj as string;
                //Pub/sub complex string = $len\r\nStr\r\nStr...\r\n
                message.Add($"${complex.Length}\r\n{complex}\r\n");
            }

            Message = message;
        }

        public static implicit operator PipePublication(string str)
        {
            return new PipePublication(str);
        }

        public override string ToString()
        {
            return string.Join("\r\n", Message);
        }
    }
}
