using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrystalTable.Data;

namespace CrystalTable.Tests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void WaferInfo_DefaultValues()
        {
            var info = new WaferInfo();
            Assert.AreEqual<uint>(0, info.SizeX);
            Assert.AreEqual<uint>(0, info.SizeY);
            Assert.AreEqual<uint>(0, info.WaferDiameter);
        }
    }
}
