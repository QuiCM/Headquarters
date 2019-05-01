using Headquarters_Core;
using Headquarters_Core.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research_And_Development_Core
{
    [TestClass]
    public class HQContextTests
    {
        HQContext _hqC;

        [TestMethod]
        public async Task TestRegisterTypeSuccessAsync()
        {
            _hqC = new HQContext();
            await _hqC.RegisterTypeAsync(typeof(TestContext.TestCommandClass));

            Assert.AreEqual(1, _hqC.BuilderContext.Metadata.Count());

            CommandMetadata metadata = _hqC.BuilderContext.Metadata.First();
            Assert.AreEqual(nameof(TestContext.TestCommandClass.TestCommandMethod), metadata.Name);
            Assert.AreEqual(3, metadata.Parameters.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(BuildStateException))]
        public async Task TestRegisterTypeFailAsync()
        {
            _hqC = new HQContext();
            await _hqC.RegisterTypeAsync(typeof(HQContextTests));

            Assert.AreEqual(0, _hqC.BuilderContext.Metadata.Count());
        }
    }
}
