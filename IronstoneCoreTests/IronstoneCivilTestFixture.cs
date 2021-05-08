using System;
using System.IO;
using System.Reflection;
using Jpp.AcTestFramework;

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
            if (File.Exists(LogHelper.GetLogName()))
            {
                File.Delete(LogHelper.GetLogName());
            }
        }
    }
}
