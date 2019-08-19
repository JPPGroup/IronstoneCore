using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Jpp.Ironstone.Core.Autocad;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.Autocad
{
    [TestFixture(@"..\..\..\Ironstone Core Tests\Drawings\PolylineExTests1.dwg", 5)]
    [TestFixture(@"..\..\..\Ironstone Core Tests\Drawings\PolylineExTests2.dwg", 1)]
    [TestFixture(@"..\..\..\Ironstone Core Tests\Drawings\PolylineExTests3.dwg", 2)]
    [TestFixture(@"..\..\..\Ironstone Core Tests\Drawings\PolylineExTests4.dwg", 6)]
    public class PolylineExtensionTests : IronstoneTestFixture
    {
        private readonly int _polyLineSegments;

        public PolylineExtensionTests() : base(Assembly.GetExecutingAssembly(), typeof(PolylineExtensionTests)) { }
        public PolylineExtensionTests(string drawingFile, int polyLineSegments) : base(Assembly.GetExecutingAssembly(), typeof(PolylineExtensionTests), drawingFile)
        {
            _polyLineSegments = polyLineSegments;
        }

        [Test]
        public void VerifyExplodeAndErase()
        {
            var result = RunTest<int>(nameof(VerifyExplodeAndEraseResident));
            Assert.AreEqual(_polyLineSegments, result, "Incorrect number of segments from polyline.");
        }

        public int VerifyExplodeAndEraseResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var pLineCount = 0;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var pLine = GetPolylineFromDrawing();
                if (pLine == null) return pLineCount;

                pLineCount = pLine.ExplodeAndErase().Count;
                acTrans.Abort();
            }

            return pLineCount;
        }

        private static Polyline GetPolylineFromDrawing()
        {
            var dwg = Application.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;
            var res = ed.SelectAll();

            if (res.Status != PromptStatus.OK) return null;
            if (res.Value == null || res.Value.Count != 1) return null;

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                return (Polyline)acTrans.GetObject(res.Value[0].ObjectId, OpenMode.ForWrite);
            }
        }
    }
}
