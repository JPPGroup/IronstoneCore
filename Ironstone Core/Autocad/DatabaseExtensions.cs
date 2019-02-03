using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class DatabaseExtensions
    {
        public static BlockTableRecord GetModelSpace(this Database currentDatabase)
        {
            Transaction acTrans = currentDatabase.TransactionManager.TopTransaction;
            if(acTrans == null)
                throw new TransactionException("No top transaction");
            
            ObjectId modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(currentDatabase);
            BlockTableRecord modelSpace = acTrans.GetObject(modelSpaceId, OpenMode.ForRead) as BlockTableRecord;

            return modelSpace;
        }

        public static Layout GetLayout(this Database currentDatabase, string Name)
        {
            Transaction acTrans = currentDatabase.TransactionManager.TopTransaction;
            if (acTrans == null)
                throw new TransactionException("No top transaction");

            //DBDictionary lays = acTrans.GetObject(currentDatabase.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
            LayoutManager acLayoutMgr = LayoutManager.Current;
            ObjectId layoutId = acLayoutMgr.GetLayoutId(Name);

            /*// Step through and list each named layout and Model
            foreach (DBDictionaryEntry item in lays)
                {
                    item.k
                }
            }*/

            return acTrans.GetObject(layoutId, OpenMode.ForRead) as Layout;
        }
    }
}
