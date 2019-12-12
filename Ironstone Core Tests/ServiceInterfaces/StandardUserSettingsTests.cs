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
                Assert.IsTrue(RunTest<bool>(nameof(VerifyBaseValueResident)));
                Assert.IsTrue(RunTest<bool>(nameof(VerifyTopUserResident)));
            });
        }

        public bool VerifyBaseValueResident()
        {
            IUserSettings settings = CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            return settings.GetValue("standarddetaillibrary.location").Equals("N:\\Consulting\\Library\\Ironstone\\Details");
        }

        public bool VerifyTopUserResident()
        {
            IUserSettings settings = CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            return settings.GetValue("standarddetaillibrary.cachedisabled").Equals("false");
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
