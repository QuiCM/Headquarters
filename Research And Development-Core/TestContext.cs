using Headquarters_Core.Builders;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
