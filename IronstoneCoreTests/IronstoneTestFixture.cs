using System;
using System.IO;
using System.Reflection;
using Jpp.AcTestFramework;
using Jpp.Ironstone.Core.Mocking;

namespace Jpp.Ironstone.Core.Tests
{
    public abstract class IronstoneTestFixture : CoreConsoleTestFixture
    {
        private const string CORE_LIBRARY = "IronstoneCore.dll";
        

        protected IronstoneTestFixture(Assembly fixtureAssembly, Type fixtureType) : base(
            new CoreConsoleFixtureArguments(fixtureAssembly, fixtureType, CORE_LIBRARY) { ClientTimeout = 480000 }) { }//fixtureAssembly, fixtureType, CORE_LIBRARY, DEBUG) { }
        protected IronstoneTestFixture(Assembly fixtureAssembly, Type fixtureType, string drawingFile) : base(
            new CoreConsoleFixtureArguments(fixtureAssembly, fixtureType, CORE_LIBRARY) {DrawingFile = drawingFile})
        { }// : base(fixtureAssembly, fixtureType, drawingFile, CORE_LIBRARY, DEBUG) { }

        public override void Setup()
        {
            var config = new Configuration();
            config.TestSettings();
            ConfigurationHelper.CreateConfiguration(config);

            if (File.Exists(config.LogFile))
            {
                File.Delete(config.LogFile);
            }
        }
    }
}
