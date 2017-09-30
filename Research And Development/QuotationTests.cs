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
    public class QuotationTests
    {
        const string M1 = "\"this is a quoted message\"";
        const string M2 = "this is not";
        const string Output = "this is a quoted message";

        [CommandClass]
        public class TestCommand
        {
            [CommandExecutor("Test command", "quote me {message1} {message2}", RegexStringOptions.MatchFromStart)]
            public object Execute(IContextObject context, string message1, string message2)
            {
                Console.WriteLine(message2);
                return message1;
            }
        }

        [TestMethod]
        public void TestQuotationParsing()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                registry.AddCommand(typeof(TestCommand));

                object testOutput = null;
                registry.HandleInput($"quote me {M1} {M2}", null, (result, output) => { testOutput = output; mre.Set(); });

                mre.WaitOne();

                Assert.AreEqual(Output, testOutput);
            }
        }
    }
}
