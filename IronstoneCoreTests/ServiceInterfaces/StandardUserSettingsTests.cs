using System;
using System.Diagnostics;
using Jpp.Ironstone.Core.ServiceInterfaces;
using NUnit.Framework;
using System.Reflection;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces
{
    [TestFixture]
    class StandardUserSettingsTests : IronstoneTestFixture
    {
        public StandardUserSettingsTests() : base(Assembly.GetExecutingAssembly(), typeof(StandardUserSettingsTests)) { }

        /*[Test]
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

        [TestCase("overwritetest.setting1", ExpectedResult="original")] //Test base setting
        [TestCase("overwritetest.setting2", ExpectedResult = "modified")] //Test overwrite
        [TestCase("oveRwritEtest.settiNg1", ExpectedResult = "original")] //Test mixed case
        [TestCase("OVERWRITETEST.SETTING1", ExpectedResult = "original")] //Test upper case
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

        [TestCase("casttest.success", ExpectedResult = CastResult.CastSucceeded)]
        [TestCase("casttest.fail", ExpectedResult = CastResult.CastFailed)]
        public CastResult VerifyCast(string key)
        {
            return RunTest<CastResult>(nameof(VerifyCastResident), key);
        }

        public CastResult VerifyCastResident(string key)
        {
            StandardUserSettings settings = (StandardUserSettings)CoreExtensionApplication._current.Container.Resolve<IUserSettings>();
            try
            {
                bool? result = settings.GetValue<bool>(key);
                if(result.HasValue)
                    return CastResult.CastSucceeded;

                throw new InvalidOperationException("Key not found");
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
        }*/
    }
}
