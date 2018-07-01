using HQ;
using HQ.Attributes;
using HQ.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RnD
{
    [TestClass]
    public class ErrorHandlerTests
    {
        [CommandClass]
        public class TestCommand
        {
            [CommandExecutor(
                "Test",
                "word {obj1} {obj2}",
                HQ.RegexStringOptions.MatchFromStart,
                "word"
            )]
            public async Task<object> Execute(IContextObject context, string obj1, int obj2, Exception ex)
            {
                await Task.CompletedTask;

                return null;
            }
            
            [ErrorHandler]
            public void ErrorHandler(IContextObject context, Type expected, string providedValue, Exception ex)
            {
                Assert.AreEqual(typeof(int), expected);
                Assert.AreEqual("test", providedValue);
                Assert.IsInstanceOfType(ex, typeof(HQ.Exceptions.CommandParsingException));
            }
        }

        [TestMethod]
        public void Test()
        {
            using (CommandRegistry reg = new CommandRegistry(new RegistrySettings()))
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                reg.AddCommand(typeof(TestCommand));

                reg.HandleInput("word test test", null, (r, o) => { mre.Set(); });

                mre.WaitOne();
            }
        }
    }
}
