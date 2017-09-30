using Microsoft.VisualStudio.TestTools.UnitTesting;
using HQ.Parsing;
using HQ;
using HQ.Attributes;

namespace RnD
{
    [TestClass]
    public class LightweightParserTests
    {
        [TestMethod]
        public void LightweightParserTestSucceeds()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            {
                LightweightParser lwParser = new LightweightParser(new ContextObject(registry))
                .AddType(typeof(int))
                .AddType(typeof(float))
                .AddType((typeof(string), new CommandParameterAttribute(repetitions: -1, optional: true)))
                .Parse("1 2.0 test test test");

                Assert.AreEqual(lwParser.Get<int>(), 1);
                Assert.AreEqual(lwParser.Get<float>(), 2.0f);
                Assert.AreEqual(lwParser.Get<string>(), "test test test");

                lwParser.Parse("1 2.0");
                Assert.AreEqual(lwParser.Get<int>(), 1);
                Assert.AreEqual(lwParser.Get<float>(), 2.0f);
                Assert.IsNull(lwParser.Get<string>());
            }
        }
    }
}
