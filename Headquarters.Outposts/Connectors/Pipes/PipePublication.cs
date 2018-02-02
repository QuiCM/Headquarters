using Headquarters.Communications;
using System.Collections.Generic;
using System.Linq;

namespace Headquarters.Outposts.Connectors.Pipes
{
    public class PipePublication : IPublication
    {
        public int Length => Message.Count();

        public IEnumerable<string> Message { get; }

        public PipePublication(params object[] messages)
        {
            //Redis pubsub format:
            // *messageLength\r\n
            // :Number\r\n
            // +Simple string\r\n
            // $Complex \r\n string\r\n

            List<string> message = new List<string>
            {
                "*" + messages.Length
            };

            foreach (object obj in messages)
            {
                if (obj is int)
                {
                    //Pub/Sub int = :Num
                    message.Add(":" + obj);
                    continue;
                }

                if (obj is string str && !str.Contains("\r\n"))
                {
                    //Pub/sub simple string = +Str
                    message.Add("+" + obj);
                    continue;
                }

                //Pub/sub complex string = $Str\r\nStr
                message.Add("$" + obj);
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
