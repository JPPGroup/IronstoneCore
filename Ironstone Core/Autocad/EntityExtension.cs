using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class EntityExtension
    {
        public static void Flatten(this Entity obj)
        {
            Transaction acTrans = obj.Database.TransactionManager.TopTransaction;

            //TODO: Is this the right place, add more types?
            switch (obj)
            {
                case Polyline polyline:
                {
                    polyline.Elevation = 0;
                    break;
                }
                case Polyline2d polyline2d:
                {
                    polyline2d.Elevation = 0;
                    break;
                }
                case Polyline3d polyline3d:
                {
                    foreach (ObjectId id in polyline3d)
                    {
                        PolylineVertex3d plv3d = (PolylineVertex3d) acTrans.GetObject(id, OpenMode.ForWrite);
                        Point3d p3d = plv3d.Position;
                        plv3d.Position = new Point3d(p3d.X, p3d.Y, 0);
                    }

                    break;
                }
                default:
                    throw new NotImplementedException();
            }
        }

        public static DBObjectCollection ExplodeAndErase(this Entity entity)
        {
            DBObjectCollection acDbObjColl = new DBObjectCollection();
            Transaction acTrans = entity.Database.TransactionManager.TopTransaction;
            BlockTableRecord acBlkTblRec = (BlockTableRecord) acTrans.GetObject(entity.BlockId, OpenMode.ForWrite);

            entity.Explode(acDbObjColl);
            entity.Erase();

            foreach (Entity acEnt in acDbObjColl)
            {
                acBlkTblRec.AppendEntity(acEnt);
                acTrans.AddNewlyCreatedDBObject(acEnt, true);
            }

            return acDbObjColl;
        }
    }
}
