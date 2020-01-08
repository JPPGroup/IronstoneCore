using System;
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
        public void VerifyCast()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyCastResident)));
        }

        [Test]
        public void VerifyOverwrite()
        {
            Assert.Multiple(() =>
            {
                StringAssert.AreEqualIgnoringCase("N:\\Consulting\\Library\\Ironstone\\Details", RunTest<string>(nameof(VerifyBaseValueResident)));
                StringAssert.AreEqualIgnoringCase("false", RunTest<string>(nameof(VerifyTopUserResident)));
            });
        }

        public bool VerifyCastResident()
        {
            try
            {
                StandardUserSettings settings = (StandardUserSettings)CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public string VerifyBaseValueResident()
        {
            StandardUserSettings settings = (StandardUserSettings) CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            return settings.GetValue("standarddetaillibrary.location");
        }

        public string VerifyTopUserResident()
        {
            StandardUserSettings settings = (StandardUserSettings)CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            return settings.GetValue("standarddetaillibrary.cachedisabled");
        }

        [Test]
        public void VerifyMissing()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyMissingResident)));
        }

        public bool VerifyMissingResident()
        {
            StandardUserSettings settings = (StandardUserSettings)CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            return settings.GetValue("junk.data") == null;
        }

        [Test]
        public void VerifySameLoadedInstance()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifySameLoadedInstanceResident)));
        }

        public bool VerifySameLoadedInstanceResident()
        {
            StandardUserSettings settings = (StandardUserSettings)CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            return settings.GetValue("junk.data") == null;
        }
    }
}
