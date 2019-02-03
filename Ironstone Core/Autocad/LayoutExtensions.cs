using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class LayoutExtensions
    {
        public static void GetBlock(this Layout currentLayout)
        {
            Transaction acTrans = currentLayout.Database.TransactionManager.TopTransaction;
            if (acTrans == null)
                throw new TransactionException("No top transaction");

            BlockTableRecord btr =
                acTrans.GetObject(currentLayout.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;

            var ids = btr.GetBlockReferenceIds(true, false);

            /*    int cnt = ids.Count;
                for (int i = 0; i < cnt; i++)
                {
                    yield return (BlockReference)
                        tr.GetObject(ids[i], mode, false, false);
                }
                if (btr.IsDynamicBlock)
                {
                    BlockTableRecord btr2 = null;
                    var blockIds = btr.GetAnonymousBlockIds();
                    cnt = blockIds.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        btr2 = (BlockTableRecord)tr.GetObject(blockIds[i],
                            OpenMode.ForRead, false, false);
                        ids = btr2.GetBlockReferenceIds(directOnly, true);
                        int cnt2 = ids.Count;
                        for (int j = 0; j < cnt2; j++)
                        {
                            yield return (BlockReference)
                                tr.GetObject(ids[j], mode, false, false);
                        }
                    }
                }*/
        }
    }
}
