using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class DatabaseExtensions
    {
        public static BlockTableRecord GetModelSpace(this Database currentDatabase, bool forWrite = false)
        {
            Transaction acTrans = currentDatabase.TransactionManager.TopTransaction;
            if (acTrans == null) throw new TransactionException("No top transaction");

            ObjectId modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(currentDatabase);
            OpenMode mode = forWrite ? OpenMode.ForWrite : OpenMode.ForRead;

            BlockTableRecord modelSpace = acTrans.GetObject(modelSpaceId, mode) as BlockTableRecord;

            return modelSpace;
        }

        /// <summary>
        /// Get layout identified by name from the specified database
        /// </summary>
        /// <param name="currentDatabase">Database layout is in</param>
        /// <param name="Name">Case-insensitive layout name</param>
        /// <returns>The request layout if found, otherwise null</returns>
        public static Layout GetLayout(this Database currentDatabase, string Name)
        {
            Transaction acTrans = currentDatabase.TransactionManager.TopTransaction;
            if (acTrans == null)
                throw new TransactionException("No top transaction");
            
            Database old = Application.DocumentManager.MdiActiveDocument.Database;
            HostApplicationServices.WorkingDatabase = currentDatabase;

            LayoutManager acLayoutMgr = LayoutManager.Current;
            ObjectId layoutId = acLayoutMgr.GetLayoutId(Name);

            HostApplicationServices.WorkingDatabase = old;

            if (layoutId.IsNull)
                return null;

            return acTrans.GetObject(layoutId, OpenMode.ForRead) as Layout;
        }

        public static void RemoveAllLayouts(this Database currentDatabase)
        {
            Transaction acTrans = currentDatabase.TransactionManager.TopTransaction;

            //Remove all sheets
            ObjectId newLayout = LayoutManager.Current.CreateLayout("Paper");
            LayoutManager.Current.SetCurrentLayoutId(newLayout);
            DBDictionary lays =
                acTrans.GetObject(currentDatabase.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;
            foreach (DBDictionaryEntry item in lays)
            {
                string layoutName = item.Key;
                if (layoutName != "Model" && layoutName != "Paper")
                {
                    LayoutManager.Current.DeleteLayout(layoutName);
                }
            }
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

        public static void PurgeAll(this Database currentDatabase)
        {
            Transaction acTrans = currentDatabase.TransactionManager.TopTransaction;
            bool toBePurged = true;

            while (toBePurged)
            {
                // Create the list of objects to "purge"
                ObjectIdCollection collection = new ObjectIdCollection();

                LayerTable lt = acTrans.GetObject(currentDatabase.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (ObjectId layer in lt)
                {
                    collection.Add(layer);
                }

                LinetypeTable ltt =
                    acTrans.GetObject(currentDatabase.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                foreach (ObjectId linetype in ltt)
                {
                    collection.Add(linetype);
                }

                TextStyleTable tst =
                    acTrans.GetObject(currentDatabase.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                foreach (ObjectId text in tst)
                {
                    collection.Add(text);
                }

                BlockTable bt = acTrans.GetObject(currentDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                foreach (ObjectId block in bt)
                {
                    collection.Add(block);
                }

                DBDictionary tsd =
                    acTrans.GetObject(currentDatabase.TableStyleDictionaryId, OpenMode.ForRead) as DBDictionary;
                foreach (DBDictionaryEntry ts in tsd)
                {
                    collection.Add(ts.Value);
                }

                // Call the Purge function to filter the list
                currentDatabase.Purge(collection);

                if (collection.Count > 0)
                {
                    // Erase each of the objects we've been allowed to
                    foreach (ObjectId id in collection)
                    {
                        DBObject obj = acTrans.GetObject(id, OpenMode.ForWrite);
                        obj.Erase();
                    }
                }
                else
                {
                    toBePurged = false;
                }
            }
        }

        internal static List<BlockTableRecord> GetAllBlockDefinitions(this Database currentDatabase)
        {
            Transaction acTrans = currentDatabase.TransactionManager.TopTransaction;
            List<BlockTableRecord> result = new List<BlockTableRecord>();

            BlockTable blkTable = (BlockTable)acTrans.GetObject(currentDatabase.BlockTableId, OpenMode.ForRead);
            foreach (ObjectId id in blkTable)
            {
                BlockTableRecord btRecord = (BlockTableRecord)acTrans.GetObject(id, OpenMode.ForRead);
                if (!btRecord.IsLayout)
                {
                    result.Add(btRecord);
                }
            }

            return result;
        }

        internal static List<BlockReference> GetAllBlockReferences(this Database currentDatabase)
        {
            Transaction acTrans = currentDatabase.TransactionManager.TopTransaction;
            List<BlockReference> result = new List<BlockReference>();

            foreach (BlockTableRecord btRecord in GetAllBlockDefinitions(currentDatabase))
            {
                var ids = btRecord.GetBlockReferenceIds(true, true);
                int cnt = ids.Count;

                for (int i = 0; i < cnt; i++)
                {
                    result.Add((BlockReference) acTrans.GetObject(ids[i], OpenMode.ForRead, false, false));
                }

                if (btRecord.IsDynamicBlock)
                {
                    BlockTableRecord btr2 = null;
                    var blockIds = btRecord.GetAnonymousBlockIds();
                    cnt = blockIds.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        btr2 = (BlockTableRecord) acTrans.GetObject(blockIds[i],
                            OpenMode.ForRead, false, false);
                        ids = btr2.GetBlockReferenceIds(true, true);
                        int cnt2 = ids.Count;
                        for (int j = 0; j < cnt2; j++)
                        {
                            result.Add((BlockReference) acTrans.GetObject(ids[j], OpenMode.ForRead, false, false));
                        }
                    }
                }
            }

            return result;
        }
    }
}
