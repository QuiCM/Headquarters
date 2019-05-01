using Headquarters_Core;
using Headquarters_Core.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Research_And_Development_Core
{
    public class TestContext
    {
        public static CommandBuilder Builder { get; }
        public static CommandBuilderContext BuilderContext { get; } = new CommandBuilderContext();

        static TestContext()
        {
            Builder = new CommandBuilder(BuilderContext);
        }


        public class TestCommandClass
        {
            [HQCommand]
            public void TestCommandMethod([HQCommandParameter] int param1, int param2, string param3)
            {

            }
        }
    }
}
