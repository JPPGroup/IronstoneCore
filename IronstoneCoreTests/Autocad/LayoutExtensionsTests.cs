using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.Autocad
{
    [TestFixture]
    class LayoutExtensionsTests : IronstoneTestFixture
    {
        public LayoutExtensionsTests() : base(Assembly.GetExecutingAssembly(), typeof(LayoutExtensionsTests),
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Test Drawings\\CivilTemplate.dwg")
        { }

        [Test]
        public void VerifyBlockReferencesOnKnownLayout()
        {
            int blocksFound = RunTest<int>(nameof(VerifyBlockReferencesOnKnownLayoutResident));
            Assert.AreEqual(3, blocksFound);
        }

        public int VerifyBlockReferencesOnKnownLayoutResident()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                Layout layout = doc.Database.GetLayout("CIV_A0L");
                IEnumerable<BlockReference> references = layout.GetBlockReferences();
                return references.Count();
            }
        }
    }
}
