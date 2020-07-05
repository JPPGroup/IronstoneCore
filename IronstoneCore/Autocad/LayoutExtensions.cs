using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class LayoutExtensions
    {
        public static List<BlockReference> GetBlockReferences(this Layout currentLayout)
        {
            Transaction acTrans = currentLayout.Database.TransactionManager.TopTransaction;
            if (acTrans == null)
                throw new TransactionException("No top transaction");

            BlockTableRecord btr =
                acTrans.GetObject(currentLayout.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;

            RXClass blockRef = RXClass.GetClass(typeof(BlockReference));
            List<BlockReference> collection = new List<BlockReference>();

            foreach (ObjectId obj in btr)
            {
                if (obj.ObjectClass.IsDerivedFrom(blockRef))
                {
                    collection.Add((BlockReference) acTrans.GetObject(obj, OpenMode.ForRead));
                }
            }

            return collection;
        }
    }
}
