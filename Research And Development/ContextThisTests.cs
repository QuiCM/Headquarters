using HQ;
using HQ.Attributes;
using HQ.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RnD
{
    [TestClass]
    public class ContextThisTests
    {
        const int Output = 5;
        const string ThisName = "unit-test";
        static readonly Type ThisType = typeof(int);

        [CommandClass]
        public class TestCommand
        {
            /// <summary>
            /// A test executor for this command.
            /// </summary>
            /// <param name="context"></param>
            /// <param name="word">parsed from {word}</param>
            /// <param name="word2">parsed from {word2}</param>
            /// <param name="num">defaults to 1 so that if not parsed correctly from the input string, the test will fail</param>
            /// <returns></returns>
            [CommandExecutor("A unit testing command",
                @"unit-test",
                RegexStringOptions.None,
                "unit-test")]
            public object TestExecutor(IContextObject context)
            {
                return 0;
            }

            [SubcommandExecutor(nameof(TestExecutor), "Unit testing subcommand", "string", RegexStringOptions.None)]
            public object TestStringExecutor(IContextObject context)
            {
                return context[ThisName];
            }

            [SubcommandExecutor(nameof(TestExecutor), "Unit testing subcommand", "type", RegexStringOptions.None)]
            public object TestTypeExecutor(IContextObject context)
            {
                return context[ThisType];
            }
        }

        [TestMethod]
        public void TestEndToEndStringUsage()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                registry.AddCommand(typeof(TestCommand));

                ContextObject context = new ContextObject(registry);
                context[ThisName] = Output;

                dynamic testOutput = null;
                registry.HandleInput($"unit-test string", context, (result, output) => { testOutput = output; mre.Set(); });

                mre.WaitOne();

                Assert.AreEqual(Output, testOutput);
            }
        }

        [TestMethod]
        public void TestEndToEndTypeUsage()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                registry.AddCommand(typeof(TestCommand));

                ContextObject context = new ContextObject(registry);
                context[ThisType] = Output;

                dynamic testOutput = null;
                registry.HandleInput($"unit-test type", context, (result, output) => { testOutput = output; mre.Set(); });

                mre.WaitOne();

                Assert.AreEqual(Output, testOutput);
            }
        }
    }
}
