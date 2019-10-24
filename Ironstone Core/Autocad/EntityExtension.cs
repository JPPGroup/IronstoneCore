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

            if (entity is Polyline || entity is Polyline3d || entity is Polyline2d) //Only stable test with polylines - line, arc, etc appear unstable.
            {
                entity.Explode(acDbObjColl);
                entity.Erase();

                foreach (Entity acEnt in acDbObjColl)
                {
                    acBlkTblRec.AppendEntity(acEnt);
                    acTrans.AddNewlyCreatedDBObject(acEnt, true);
                }

                return acDbObjColl;
            }

            acDbObjColl.Add(entity);

            return acDbObjColl;
        }

        public static Polyline ToPolyline(this Entity entity)
        {
            Polyline polyline = new Polyline();
            Plane plane = new Plane(Point3d.Origin, Vector3d.ZAxis);
            double bulge = 0.0;

            Point2d startPoint;
            Point2d endPoint;

            switch (entity)
            {
                case Arc arc:
                    startPoint = arc.StartPoint.Convert2d(plane);
                    endPoint = arc.EndPoint.Convert2d(plane);
                    bulge = arc.Bulge(plane);
                    break;
                case Line line:
                    startPoint = line.StartPoint.Convert2d(plane);
                    endPoint = line.EndPoint.Convert2d(plane);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entity), entity, @"Type not handled");
            }

            polyline.AddVertexAt(0, startPoint, bulge, 0.0, 0.0);
            polyline.AddVertexAt(1, endPoint, 0.0, 0.0, 0.0);

            return polyline;
        }
    }
}
