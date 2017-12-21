using System;

namespace Headquarters.Outposts
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string path = "";
            string cmdLine = "";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-c" || args[i] == "--config")
                {
                    if (args.Length >= 2)
                    {
                        path = args[++i];
                    }
                }
                else
                {
                    cmdLine += args[i];
                }
            }

            using (Outpost outpost = new Outpost(cmdLine))
            {
                if (path != null)
                {
                    outpost.ReadConfig(path);
                }

                await outpost.ConnectAsync();
            }
        }
    }
}
