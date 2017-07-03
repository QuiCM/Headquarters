using HQ;
using HQ.Interfaces;
using HQ.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace RnD
{
    [TestClass]
    public class BasicScannerTests
    {
        const string ScannerOutput = "Hello scanner";
        [TestMethod]
        public void TestScanner()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            {
                ManualResetEvent mre = new ManualResetEvent(false);

                object output = null;
                registry.AddScanner(@"Scanner \w+ \d+", (IContextObject ctx, string match, LightweightParser parser, ref bool finalize) =>
                {
                    output = ScannerOutput;
                    mre.Set();
                    return ScannerOutput;
                });

                registry.HandleInput("Scanner pattern 123", null, null);

                mre.WaitOne();

                Assert.AreEqual(output, ScannerOutput);
            }
        }
    }
}
