using Jpp.Ironstone.Core.ServiceInterfaces;
using NUnit.Framework;
using System.Reflection;
using Unity;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces
{
    [TestFixture]
    class StandardUserSettingsTests : IronstoneTestFixture
    {
        public StandardUserSettingsTests() : base(Assembly.GetExecutingAssembly(), typeof(StandardUserSettingsTests)) { }

        [Test]
        public void VerifyOverwrite()
        {
            Assert.Multiple(() =>
            {
                StringAssert.AreEqualIgnoringCase("N:\\Consulting\\Library\\Ironstone\\Details", RunTest<string>(nameof(VerifyBaseValueResident)));
                StringAssert.AreEqualIgnoringCase("false", RunTest<string>(nameof(VerifyTopUserResident)));
            });
        }

        public string VerifyBaseValueResident()
        {
            IUserSettings settings = CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            return settings.GetValue("standarddetaillibrary.location");
        }

        public string VerifyTopUserResident()
        {
            IUserSettings settings = CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            return settings.GetValue("standarddetaillibrary.cachedisabled");
        }

        [Test]
        public void VerifyMissing()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyMissingResident)));
        }

        public bool VerifyMissingResident()
        {
            IUserSettings settings = CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            return string.IsNullOrEmpty(settings.GetValue("junk.data"));
        }
    }
}
