using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using NUnit.Framework;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.Tests.Autocad
{
    [TestFixture]
    class DatabaseExtensionTests : IronstoneTestFixture
    {
        public DatabaseExtensionTests() : base(Assembly.GetExecutingAssembly(), typeof(DatabaseExtensionTests),
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Test Drawings\\CivilTemplate.dwg")
        { }

        [Test]
        public void VerifyReturnedBlockDefinitionCount()
        {
            int blocksFound = RunTest<int>(nameof(VerifyReturnedBlockDefinitionCountResident));
            Assert.AreEqual(55, blocksFound);
        }

        public int VerifyReturnedBlockDefinitionCountResident()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                List<BlockTableRecord> records = doc.Database.GetAllBlockDefinitions();
                return records.Count;
            }
        }

        [Test]
        public void VerifyReturnedBlockReferenceCount()
        {
            int blocksFound = RunTest<int>(nameof(VerifyReturnedBlockReferenceCountResident));
            Assert.AreEqual(156, blocksFound);
        }

        public int VerifyReturnedBlockReferenceCountResident()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                List<BlockReference> records = doc.Database.GetAllBlockReferences();
                return records.Count;
            }
        }

        [TestCase("CIV_A0L", ExpectedResult = true)]
        [TestCase("CIV_A1L", ExpectedResult = true)]
        [TestCase("CIV_A2L", ExpectedResult = true)]
        [TestCase("CIV_A3L", ExpectedResult = true)]
        [TestCase("CIV_A4P", ExpectedResult = true)]
        [TestCase("CIV_A0P", ExpectedResult = true)]
        [TestCase("CIV_A1P", ExpectedResult = true)]
        [TestCase("CIV_A2P", ExpectedResult = true)]
        [TestCase("CIV_A3P", ExpectedResult = true)]
        [TestCase("Civ_A3P", ExpectedResult = true)]
        [TestCase("civ_a3p", ExpectedResult = true)]
        [TestCase("unknown", ExpectedResult = false)]
        public bool TestLayoutRetrieval(string layoutName)
        {
            return RunTest<bool>(nameof(TestLayoutRetrievalResident), layoutName);
        }

        public bool TestLayoutRetrievalResident(string layoutName)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                Layout layout = doc.Database.GetLayout(layoutName);

                if (layout == null)
                    return false;
            }

            return true;
        }
    }
}
