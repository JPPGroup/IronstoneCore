using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Jpp.Ironstone.Core.Autocad;
using NUnit.Framework;
using System;
using System.Reflection;

namespace Jpp.Ironstone.Core.Tests.Autocad
{
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests1.dwg", 5, TestName = "Polyline with 5 vertices")]
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests2.dwg", 1, TestName = "Polyline with 1 vertices")]
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests3.dwg", 2, TestName = "Polyline with 2 vertices")]
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests4.dwg", 6, TestName = "Polyline with 6 vertices")]
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests5.dwg", 1, TestName = "Circle")]
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests6.dwg", 1, TestName = "Line")]
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests7.dwg", 1, TestName = "Arc")]
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests8.dwg", 4, TestName = "3d Polyline with 4 vertices")]
    [TestFixture(@"..\..\..\Test-Drawings\EntityExtensionTests9.dwg", 4, TestName = "2d Polyline with 4 vertices")]
    public class EntityExtensionTests : IronstoneTestFixture
    {
        private readonly int _expectedEntities;

        public EntityExtensionTests() : base(Assembly.GetExecutingAssembly(), typeof(EntityExtensionTests)) { }
        public EntityExtensionTests(string drawingFile, int expectedEntities) : base(Assembly.GetExecutingAssembly(), typeof(EntityExtensionTests), drawingFile)
        {
            _expectedEntities = expectedEntities;
        }

        [Test]
        public void VerifyEntityFromDrawingExplodeAndErase()
        {
            var result = RunTest<int>(nameof(VerifyEntityFromDrawingExplodeAndEraseResident));
            Assert.AreEqual(_expectedEntities, result, "Incorrect number of entities from explode.");
        }

        [Test]
        public void VerifyToPolylineWhenNotArcOrLine_Throw_Exception_Test()
        {
            var result = RunTest<bool>(nameof(VerifyToPolylineWhenNotArcOrLine_Throw_Exception_TestResident));
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyToPolylineWhenArcOrLine()
        {
            var result = RunTest<bool>(nameof(VerifyToPolylineWhenArcOrLineResident));
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyFlattenWhenNotPolyline_Throw_Exception_Test()
        {
            var result = RunTest<bool>(nameof(VerifyFlattenWhenNotPolyline_Throw_Exception_TestResident));
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyFlattenWhenPolyline()
        {
            var result = RunTest<bool>(nameof(VerifyFlattenWhenPolylineResident));
            Assert.IsTrue(result);
        }


        public static int VerifyEntityFromDrawingExplodeAndEraseResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var entityCount = 0;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var entity = GetEntityFromDrawing(acDoc);
                if (entity == null) return entityCount;

                entityCount = entity.ExplodeAndErase().Count;
                acTrans.Abort();
            }

            return entityCount;
        }

        public static bool VerifyToPolylineWhenArcOrLineResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                try
                {
                    var entity = GetEntityFromDrawing(acDoc);
                    if (entity == null) return false;

                    switch (entity)
                    {
                        case Arc arc:
                        {
                            var pLine = entity.ToPolyline();
                            var segment = pLine.GetArcSegmentAt(0);

                            return pLine.StartPoint == arc.StartPoint && 
                                   pLine.EndPoint == arc.EndPoint &&
                                   Math.Abs(segment.Radius - arc.Radius) < 0.00001;
                        }
                        case Line line:
                        {
                            var pLine = entity.ToPolyline();

                            return pLine.StartPoint == line.StartPoint &&
                                   pLine.EndPoint == line.EndPoint;
                        }
                        default:
                            return true;
                    }
                    
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    acTrans.Abort();
                }
            }
        }

        public static bool VerifyToPolylineWhenNotArcOrLine_Throw_Exception_TestResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                try
                {
                    var entity = GetEntityFromDrawing(acDoc);
                    if (entity == null) return false;

                    if (entity is Arc || entity is Line) return true;

                    var _ = entity.ToPolyline();
                    return false;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    acTrans.Abort();
                }
            }
        }

        public static bool VerifyFlattenWhenPolylineResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                try
                {
                    var entity = GetEntityFromDrawing(acDoc);
                    if (entity == null) return false;

                    switch (entity)
                    {
                        case Polyline polyline:
                        {
                            polyline.Flatten();
                            return polyline.Elevation.Equals(0);
                        }
                        case Polyline2d polyline2d:
                        {
                            polyline2d.Flatten();
                            return polyline2d.Elevation.Equals(0);
                        }
                        case Polyline3d polyline3d:
                        {
                            polyline3d.Flatten();
                            foreach (ObjectId id in polyline3d)
                            {
                                var plv3d = (PolylineVertex3d) acTrans.GetObject(id, OpenMode.ForRead);
                                if (Math.Abs(plv3d.Position.Z) > 0) return false;
                            }
                            return true;
                        }
                        default:
                            return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    acTrans.Abort();
                }
            }
        }



        public static bool VerifyFlattenWhenNotPolyline_Throw_Exception_TestResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                try
                {
                    var entity = GetEntityFromDrawing(acDoc);
                    if (entity == null) return false;

                    if (entity is Polyline || entity is Polyline2d || entity is Polyline3d) return true;

                    entity.Flatten();
                    return false;
                }
                catch (NotImplementedException)
                {
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    acTrans.Abort();
                }
            }
        }

        private static Entity GetEntityFromDrawing(Document doc)
        {
            var acTrans = doc.Database.TransactionManager.TopTransaction;

            var ed = doc.Editor;
            var res = ed.SelectAll();

            if (res.Status != PromptStatus.OK) return null;
            if (res.Value == null || res.Value.Count != 1) return null;

            return (Entity)acTrans.GetObject(res.Value[0].ObjectId, OpenMode.ForWrite);
        }
    }
}
