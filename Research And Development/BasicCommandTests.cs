using HQ;
using HQ.Attributes;
using HQ.Interfaces;
using HQ.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace RnD
{

    [TestClass]
    public class BasicCommandTests
    {
        const string CommandOutput = "Hello unit test!";

        [CommandClass]
        public class TestCommand
        {
            [CommandExecutor("A unit testing command", "unit-test", RegexStringOptions.None, "unit-test")]
            public object TestExecutor(IContextObject context)
            {
                return CommandOutput;
            }
        }

        [TestMethod]
        public void TestVerification()
        {
            //Asserts that the verifier can successfully handle the command without throwing an exception
            FormatVerifier verifier = new FormatVerifier(typeof(TestCommand)).Run();
        }

        [TestMethod]
        public void TestAddition()
        {
            //Asserts that a command registry can register the command without throwing an exception
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            {
                registry.AddCommand(typeof(TestCommand));
            }
        }

        [TestMethod]
        public void TestExecution()
        {
            //Asserts that a command can be run and returns the expected result

            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            {
                registry.AddCommand(typeof(TestCommand));

                ManualResetEvent mre = new ManualResetEvent(false);

                object testOutput = null;
                registry.HandleInput("unit-test", null, (result, output) => { testOutput = output; mre.Set(); });

                mre.WaitOne();

                Assert.AreEqual(testOutput, CommandOutput);
            }
        }
    }
}
