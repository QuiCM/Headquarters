using HQ;
using HQ.Attributes;
using HQ.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace RnD
{
    [TestClass]
    public class SubcommandTests
    {
        [CommandClass]
        public class TestCommand
        {
            [CommandExecutor("Test root command", "unit-test", HQ.RegexStringOptions.MatchFromStart, "unit-test")]
            public object Execute(IContextObject ctx)
            {
                return -1;
            }

            [SubcommandExecutor(nameof(Execute), "sub-executor", "r(?>andom)?", HQ.RegexStringOptions.None)]
            public object SubExecute_1(IContextObject ctx)
            {
                return new Random().Next();
            }

            [SubcommandExecutor(nameof(Execute), "sub-executor", "{number}", HQ.RegexStringOptions.None)]
            public object SubExecute_2(IContextObject ctx, string number)
            {
                return "Number " + number;
            }
        }

        [TestMethod]
        public void SubcommandEndToEndTest()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                registry.AddCommand<TestCommand>();

                object testOutput = null;

                registry.HandleInput("unit-test", null, (result, output) => { testOutput = output; mre.Set(); });
                mre.WaitOne();

                Assert.AreEqual(-1, testOutput);
                mre.Reset();

                registry.HandleInput("unit-test r", null, (result, output) => { testOutput = output; mre.Set(); });
                mre.WaitOne();

                Assert.IsTrue((int)testOutput > -1);
                mre.Reset();

                registry.HandleInput("unit-test random", null, (result, output) => { testOutput = output; mre.Set(); });
                mre.WaitOne();

                Assert.IsTrue((int)testOutput > -1);
                mre.Reset();

                registry.HandleInput("unit-test 1234", null, (result, output) => { testOutput = output; mre.Set(); });
                mre.WaitOne();

                Assert.AreEqual(testOutput, "Number 1234");
            }
        }
    }
}
