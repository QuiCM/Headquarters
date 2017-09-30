using HQ;
using HQ.Attributes;
using HQ.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace RnD
{
    /// <summary>
    /// Tests that a command with a formattable trigger name (i.e., a trigger containing {word}) functions correctly
    /// </summary>
    [TestClass]
    public class FormattableTriggersTests
    {
        const string Output1 = "\"First word\"";
        const string Output2 = "\"Second word\"";
        const int Number = 3;

        private static readonly string FinalOutput = (Output1 + Output2 + Number.ToString()).Replace("\"", "");

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
                @"unit-test {word} more words {word2} *{number?}",
                RegexStringOptions.None, 
                "unit-test {word} more words {word2} {optional: number}")]
            public object TestExecutor(IContextObject context, string word, string word2, int number = 1)
            {
                return word + word2 + number.ToString();
            }

            /// <summary>
            /// This executor has 3 formattable triggers one after the other in a series. Each trigger should receive 1 argument
            /// </summary>
            /// <param name="context"></param>
            /// <param name="word1"></param>
            /// <param name="word2"></param>
            /// <param name="word3"></param>
            /// <returns></returns>
            [CommandExecutor("A unit testing command",
                @"unit-test {word1} {word2} {word3}",
                RegexStringOptions.MatchFromStart,
                "unit-test {first} {second} {third}")]
            public object TestSeriesExecutor(IContextObject context, string word1, string word2, string word3)
            {
                return word1 + word2 + word3;
            }
        }

        [TestMethod]
        public void TestEndToEndUsage()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                registry.AddCommand(typeof(TestCommand));

                object testOutput = null;
                registry.HandleInput($"unit-test {Output1} more words {Output2} {Number}", null, (result, output) => { testOutput = output; mre.Set(); });

                mre.WaitOne();

                Assert.AreEqual(FinalOutput, testOutput);
            }
        }

        [TestMethod]
        public void TestEndToEndSeriesUsage()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                registry.AddCommand(typeof(TestCommand));

                object testOutput = null;
                registry.HandleInput($@"unit-test {Output1} {Output2} {Number}", null, (result, output) => { testOutput = output; mre.Set(); });

                mre.WaitOne();

                Assert.AreEqual(FinalOutput, testOutput);
            }
        }
    }
}
