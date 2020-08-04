using NUnit.Framework;

namespace IronstoneCore.UI.UiTests
{
    [TestFixture]
    class RibbonTests
    {
        [Test]
        public void CheckTabs()
        {
            var conceptTab = Setup.CurrentDriver.FindElementByName("Ironstone Concept");
            var designTab = Setup.CurrentDriver.FindElementByName("Ironstone Design");

            Assert.IsNotNull(conceptTab);
            Assert.IsNotNull(designTab);
        }
    }
}
