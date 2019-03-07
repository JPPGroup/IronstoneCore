using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            if (obj is Polyline)
            {
                Polyline pl = obj as Polyline;
                pl.Elevation = 0;
            }
            if (obj is Polyline2d)
            {
                Polyline2d pl = obj as Polyline2d;
                pl.Elevation = 0;
            }
            if (obj is Polyline3d)
            {
                Polyline3d pl3d = obj as Polyline3d;
                foreach (ObjectId id in pl3d)
                {
                    PolylineVertex3d plv3d = acTrans.GetObject(id, OpenMode.ForWrite) as PolylineVertex3d;
                    Point3d p3d = plv3d.Position;
                    plv3d.Position = new Point3d(p3d.X, p3d.Y, 0);
                }
            }
        }
    }
}
