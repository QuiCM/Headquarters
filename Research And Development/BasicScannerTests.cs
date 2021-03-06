﻿using HQ;
using HQ.Interfaces;
using HQ.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace RnD
{
    [TestClass]
    public class BasicScannerTests
    {
        const string ScannerOutput = "Scanner pattern 123";
        const string ScannerRegex = @"Scanner (?<test>\w+) (?<test2>\d+)";

        [TestMethod]
        public void BasicScannerTestScanner()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                object output = null;
                registry.AddScanner(ScannerRegex, (IContextObject ctx, string match, LightweightParser parser) =>
                {
                    parser.AddType<string>()
                          .AddType<string>()
                          .AddType<int>();

                    parser.Parse(match);

                    output = $"{parser.Get<string>()} {parser.Get<string>()} {parser.Get<int>()}";

                    mre.Set();
                    return ScannerOutput;
                });

                registry.HandleInput(ScannerOutput, new ContextObject(registry), null);

                mre.WaitOne();

                Assert.AreEqual(output, ScannerOutput);
            }
        }

        [TestMethod]
        public void BasicScannerTestAsyncScanner()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                object output = null;
                registry.AddAsyncScanner(ScannerRegex, async (IContextObject ctx, string match, LightweightParser parser) =>
                {
                    await Task.Delay(100);

                    parser.AddType<string>()
                          .AddType<string>()
                          .AddType<int>();

                    parser.Parse(match);

                    output = $"{parser.Get<string>()} {parser.Get<string>()} {parser.Get<int>()}";

                    mre.Set();
                    return ScannerOutput;
                });

                registry.HandleInput(ScannerOutput, new ContextObject(registry), null);

                mre.WaitOne();

                Assert.AreEqual(output, ScannerOutput);
            }
        }
    }
}
