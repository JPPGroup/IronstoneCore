using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class BlockReferenceExtensions
    {
        public static string EffectiveName(this BlockReference block)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));

            if (block.IsDynamicBlock)
            {
                var trans = block.DynamicBlockTableRecord.Database.TransactionManager.TopTransaction;
                if(trans == null)
                    throw new Autodesk.AutoCAD.Runtime.Exception(ErrorStatus.NoActiveTransactions);

                return ((BlockTableRecord)trans.GetObject(block.DynamicBlockTableRecord, OpenMode.ForRead)).Name;
            }

            return block.Name;
        }
    }
}
