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
        public void VerifyConcreteResolve()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyConcreteResolveResident)));
        }

        public bool VerifyConcreteResolveResident()
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

        [TestCase("standarddetaillibrary.location", ExpectedResult="n:\\consulting\\library\\ironstone\\details")] //Test base setting
        [TestCase("standarddetaillibrary.cachedisabled", ExpectedResult = "false")] //Test overwrite
        [TestCase("standarDDetailliBrary.loCation", ExpectedResult = "n:\\consulting\\library\\ironstone\\details")] //Test mixed case
        [TestCase("STANDARDDETAILLIBRARY.LOCATION", ExpectedResult = "n:\\consulting\\library\\ironstone\\details")] //Test upper case
        public string VerifyOverwrite(string key)
        { 
            return RunTest<string>(nameof(GetSettingResident), key).ToLower();
        }

        public string GetSettingResident(string key)
        {
            StandardUserSettings settings = (StandardUserSettings) CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            return settings.GetValue(key);
        }



        [Test]
        public void VerifyRootCastFailure()
        {
            Assert.True(RunTest<bool>(nameof(VerifyRootCastFailureResident)));
        }

        public bool VerifyRootCastFailureResident()
        {
            StandardUserSettings settings = (StandardUserSettings)CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            try
            {
                settings.GetValue("standarddetaillibrary");
            }
            catch (InvalidCastException)
            {
                return true;
            }

            return false;
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
            Configuration config = CoreExtensionApplication._current.Container.Resolve<Configuration>();
            IUserSettings newSettings = settings.LoadFrom(config.NetworkUserSettingsPath);

            return object.ReferenceEquals(settings, newSettings);
        }

        [TestCase("standarddetaillibrary.cachedisabled", ExpectedResult = CastResult.CastSucceeded)]
        [TestCase("standarddetaillibrary.location", ExpectedResult = CastResult.CastFailed)]
        public CastResult VerifyCast(string key)
        {
            return RunTest<CastResult>(nameof(VerifyCastResident), key);
        }

        public CastResult VerifyCastResident(string key)
        {
            StandardUserSettings settings = (StandardUserSettings)CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            try
            {
                settings.GetValue<bool>(key);
                return CastResult.CastSucceeded;
            }
            catch (FormatException)
            {
                return CastResult.CastFailed;
            }
        }

        public enum CastResult
        {
            CastSucceeded,
            CastFailed
        }
    }
}
