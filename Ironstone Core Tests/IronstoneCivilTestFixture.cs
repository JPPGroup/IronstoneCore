using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Jpp.AcTestFramework;
using Jpp.Ironstone.Core.Mocking;

namespace Jpp.Ironstone.Core.Tests
{
    public abstract class IronstoneCivilTestFixture : Civil3dTestFixture
    {
        private const string CORE_LIBRARY = "IronstoneCore.dll";


        protected IronstoneCivilTestFixture(Assembly fixtureAssembly, Type fixtureType) : base(
            new Civil3dFixtureArguments(fixtureAssembly, fixtureType, CORE_LIBRARY) { ClientTimeout = 45000 }) {}//fixtureAssembly, fixtureType, CORE_LIBRARY, DEBUG) { }
        protected IronstoneCivilTestFixture(Assembly fixtureAssembly, Type fixtureType, string drawingFile) : base(
            new Civil3dFixtureArguments(fixtureAssembly, fixtureType, CORE_LIBRARY) {DrawingFile = drawingFile, ClientTimeout = 45000 })
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
