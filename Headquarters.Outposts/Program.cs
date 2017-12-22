using System;
using System.Threading.Tasks;

namespace Headquarters.Outposts
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string path = "";
            string provider = "";
            string cmdLine = "";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-c" || args[i] == "--config")
                {
                    if (args.Length > i + 1)
                    {
                        path = args[++i];
                    }
                }
                else if (args[i] == "-p" || args[i] == "--provider")
                {
                    if (args.Length > i + 1)
                    {
                        provider = args[++i];
                    }
                }
                else
                {
                    cmdLine += args[i];
                }
            }

            using (Outpost outpost = new Outpost(cmdLine))
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    outpost.ReadConfig(path);
                }

                if (!string.IsNullOrWhiteSpace(provider))
                {
                    outpost.SetProvider(provider);
                }

                await outpost.ConnectAsync();
            }
        }
    }
}
