using System;
using System.IO;
using System.Reflection;
using Jpp.AcTestFramework;

namespace Jpp.Ironstone.Core.Tests
{
    public abstract class IronstoneAutocadTestFixture : AutoCadTestFixture
    {
        private const string CORE_LIBRARY = "IronstoneCore.dll";
        

        protected IronstoneAutocadTestFixture(Assembly fixtureAssembly, Type fixtureType) : base(
            new AutoCadFixtureArguments(fixtureAssembly, fixtureType, CORE_LIBRARY) { ClientTimeout = 45000 }) { }//fixtureAssembly, fixtureType, CORE_LIBRARY, DEBUG) { }
        protected IronstoneAutocadTestFixture(Assembly fixtureAssembly, Type fixtureType, string drawingFile) : base(
            new AutoCadFixtureArguments(fixtureAssembly, fixtureType, CORE_LIBRARY) { ClientTimeout = 45000, DrawingFile = drawingFile})
        { }// : base(fixtureAssembly, fixtureType, drawingFile, CORE_LIBRARY, DEBUG) { }

        public override void Setup()
        {
            LogHelper.ClearLog();
        }
    }
}
