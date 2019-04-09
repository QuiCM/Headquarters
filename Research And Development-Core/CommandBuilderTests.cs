using Headquarters_Core;
using Headquarters_Core.Builders.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Research_And_Development_Core
{
    [TestClass]
    public class CommandBuilderTests
    {
        [TestMethod]
        public async Task TestCommandBuilder()
        {
            CommandMetadata metadata = (await TestContext.Builder.BuildAsync(typeof(CommandBuilderTests))).FirstOrDefault(c => c.Name == "Testcmd");

            Assert.IsTrue(metadata != null);
            Assert.AreEqual(3, metadata.Parameters.Count());
            Assert.IsTrue(metadata.Parameters.ElementAt(0).Required);
            Assert.AreEqual("Number", metadata.Parameters.ElementAt(2).Name);
        }

        [HQCommand(Name = "Testcmd")]
        private void CommandName(
            [HQCommandParameter(Name = "parameter one", Required = true)] string param1,
            string param2, 
            [HQCommandParameter(Name = "Number", Required = false, ParameterBuilder = typeof(IntParameterBuilder))] int param3)
        {
        }
    }
}
