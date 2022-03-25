using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class LayoutExtensions
    {
        public static List<BlockReference> GetBlockReferences(this Layout currentLayout)
        {
            return GetEntities<BlockReference>(currentLayout);
        }

        public static List<T> GetEntities<T>(this Layout currentLayout) where T : DBObject
        {
            Transaction acTrans = currentLayout.Database.TransactionManager.TopTransaction;
            if (acTrans == null)
                throw new TransactionException("No top transaction");

            BlockTableRecord btr =
                acTrans.GetObject(currentLayout.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;

            RXClass blockRef = RXClass.GetClass(typeof(T));
            List<T> collection = new List<T>();

            foreach (ObjectId obj in btr)
            {
                if (obj.ObjectClass.IsDerivedFrom(blockRef))
                {
                    collection.Add((T)acTrans.GetObject(obj, OpenMode.ForRead));
                }
            }

            return collection;
        }
    }
}
