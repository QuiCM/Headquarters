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
        const string Output1 = "First word";
        const string Output2 = "Second word";
        const int Number = 3;

        private static readonly string FinalOutput = Output1 + Output2 + Number.ToString();

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
            [CommandExecutor("A unit testing command", @"unit-test {word} more words {word2} and a number",
                RegexStringOptions.None, "unit-test {word} more words {word2} and a number {optional: integer}")]
            public object TestExecutor(IContextObject context, string word, string word2, int num = 1)
            {
                return word + word2 + num.ToString();
            }
        }

        [TestMethod]
        public void TestEndToEndUsage()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            {
                registry.AddCommand(typeof(TestCommand));

                ManualResetEvent mre = new ManualResetEvent(false);

                object testOutput = null;
                registry.HandleInput($"unit-test {Output1} more words {Output2} and a number {Number}", null, (result, output) => { testOutput = output; mre.Set(); });

                mre.WaitOne();

                Assert.AreEqual(FinalOutput, testOutput);
            }
        }
    }
}
