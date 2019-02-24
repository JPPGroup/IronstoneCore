using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

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

        public static LayerTableRecord GetLayer(this Database currentDatabase, string layerId)
        {
            // Start a transaction
            Transaction acTrans = currentDatabase.TransactionManager.TopTransaction;

            // Open the Layer table for read
            LayerTable acLyrTbl;
            acLyrTbl = acTrans.GetObject(currentDatabase.LayerTableId, OpenMode.ForRead) as LayerTable;

            if (acLyrTbl.Has(layerId))
            {
                return acTrans.GetObject(acLyrTbl[layerId], OpenMode.ForRead) as LayerTableRecord;
            }
            else
            {
                return null;
            }
        }

        public static void RegisterLayer(this Database currentDatabase, string layerId, short colorIndex = 7,
            string linetype = "CONTINUOUS")
        {
            // Start a transaction
            Transaction acTrans = currentDatabase.TransactionManager.TopTransaction;

            // Open the Layer table for read
            LayerTable acLyrTbl;
            acLyrTbl = acTrans.GetObject(currentDatabase.LayerTableId, OpenMode.ForRead) as LayerTable;

            if (!acLyrTbl.Has(layerId))
            {
                using (LayerTableRecord acLyrTblRec = new LayerTableRecord())
                {
                    // Assign the layer the ACI color 3 and a name
                    acLyrTblRec.Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
                    acLyrTblRec.Name = layerId;

                    LinetypeTable acLinTbl;
                    acLinTbl = acTrans.GetObject(currentDatabase.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                    if (acLinTbl.Has(linetype))
                    {
                        // Set the linetype for the layer
                        acLyrTblRec.LinetypeObjectId = acLinTbl[linetype];
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("Linetype does not exist", new Exception());
                    }

                    // Upgrade the Layer table for write
                    acLyrTbl.UpgradeOpen();

                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
                }
            }
        }

        public static void RegisterLayer(this Database currentDatabase, LayerInfo layerInfo)
        {
            RegisterLayer(currentDatabase, layerInfo.LayerId, layerInfo.IndexColor, layerInfo.Linetype);
        }
    }
}
