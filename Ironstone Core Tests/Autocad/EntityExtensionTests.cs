using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Jpp.Ironstone.Core.Autocad;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.Autocad
{
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests1.dwg", 5)]
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests2.dwg", 1)]
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests3.dwg", 2)]
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests4.dwg", 6)]
    public class EntityExtensionTests : IronstoneTestFixture
    {
        private readonly int _polyLineSegments;

        public EntityExtensionTests() : base(Assembly.GetExecutingAssembly(), typeof(EntityExtensionTests)) { }
        public EntityExtensionTests(string drawingFile, int polyLineSegments) : base(Assembly.GetExecutingAssembly(), typeof(EntityExtensionTests), drawingFile)
        {
            _polyLineSegments = polyLineSegments;
        }

        [Test]
        public void VerifyPolylineFromDrawingExplodeAndErase()
        {
            var result = RunTest<int>(nameof(VerifyPolylineFromDrawingExplodeAndEraseResident));
            Assert.AreEqual(_polyLineSegments, result, "Incorrect number of segments from polyline.");
        }

        public int VerifyPolylineFromDrawingExplodeAndEraseResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var pLineCount = 0;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var pLine = GetPolylineFromDrawing();
                if (pLine == null) return pLineCount;

                pLineCount = pLine.ExplodeAndErase().Count;
                acTrans.Commit();
            }

            return pLineCount;
        }

        private static Polyline GetPolylineFromDrawing()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acTrans = acDoc.Database.TransactionManager.TopTransaction;

            var ed = acDoc.Editor;
            var res = ed.SelectAll();

            if (res.Status != PromptStatus.OK) return null;
            if (res.Value == null || res.Value.Count != 1) return null;

            return (Polyline)acTrans.GetObject(res.Value[0].ObjectId, OpenMode.ForWrite);
        }
    }
}
