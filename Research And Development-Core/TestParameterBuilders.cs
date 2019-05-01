using Headquarters_Core.Builders.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Research_And_Development_Core
{
    [TestClass]
    public class TestParameterBuilders
    {
        static readonly IntParameterBuilder i32PB = new IntParameterBuilder(new ParameterBuilderContext());

        [TestMethod]
        public async Task TestIntParameterBuilder()
        {
            await i32PB.BuildAsync("test");
            Assert.IsTrue(i32PB.BuildState.Result == Headquarters_Core.Builders.BuildStatus.Faulted);
            Assert.AreEqual(typeof(ArgumentException), i32PB.BuildState.Exception.GetType());
            i32PB.BuildState.Reset();

            await i32PB.BuildAsync("test", "string");
            Assert.IsTrue(i32PB.BuildState.Result == Headquarters_Core.Builders.BuildStatus.Faulted);
            Assert.AreEqual(typeof(ArgumentException), i32PB.BuildState.Exception.GetType());
            i32PB.BuildState.Reset();

            await i32PB.BuildAsync("1", "string");
            Assert.IsTrue(i32PB.BuildState.Result == Headquarters_Core.Builders.BuildStatus.Faulted);
            Assert.AreEqual(typeof(ArgumentException), i32PB.BuildState.Exception.GetType());
            i32PB.BuildState.Reset();

            Assert.AreEqual(1, await i32PB.BuildAsync("1"));
        }
    }
}
