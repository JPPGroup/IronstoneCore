using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using NUnit.Framework;
using System;
using System.Reflection;


namespace Jpp.Ironstone.Core.Tests.Autocad
{
    [TestFixture(@"..\..\..\Test-Drawings\DynamicBlock.dwg")]
    public class BlockReferenceExtensionsTests : IronstoneTestFixture
    {
        private const string LevelBlockName = "ProposedLevel";

        public BlockReferenceExtensionsTests() : base(Assembly.GetExecutingAssembly(), typeof(BlockReferenceExtensionsTests)) { }
        public BlockReferenceExtensionsTests(string drawingFile) : base(Assembly.GetExecutingAssembly(), typeof(BlockReferenceExtensionsTests), drawingFile) { }


        [Test]
        public void VerifyDynamicBlockReferenceName()
        {
            var result = RunTest<bool>(nameof(VerifyDynamicBlockReferenceNameResident));
            Assert.IsTrue(result);
        }

        public static bool VerifyDynamicBlockReferenceNameResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            BlockReference block;
            
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                block = CreateDynamicBlockReference(acCurDb);
                acTrans.Commit();
            }

            if (block == null) return false;

            return block.EffectiveName() == LevelBlockName;
        }

        private static BlockReference CreateDynamicBlockReference(Database database)
        {
            var trans = database.TransactionManager.TopTransaction;
            var bt = (BlockTable)trans.GetObject(database.BlockTableId, OpenMode.ForRead);
            foreach (var btrId in bt)
            {
                var btr = (BlockTableRecord)trans.GetObject(btrId, OpenMode.ForRead);
                if (string.Equals(btr.Name, LevelBlockName, StringComparison.CurrentCultureIgnoreCase))
                {
                    var blockId = btr.ObjectId;
                    var modelSpaceRecord = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    var blockRef = new BlockReference(new Point3d(0,0,0), blockId);

                    modelSpaceRecord.AppendEntity(blockRef);
                    trans.AddNewlyCreatedDBObject(blockRef, true);

                    return blockRef;
                }
            }

            return null;
        }
    }
}
