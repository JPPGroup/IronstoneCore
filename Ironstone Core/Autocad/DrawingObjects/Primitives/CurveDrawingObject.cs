using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives
{
    /// <summary>
    /// This class is only present currently for the inheritance chain
    /// TODO: Flesh out polyline
    /// </summary>
    public abstract class CurveDrawingObject : DrawingObject
    {
        public bool IsClosed()
        {
            using (Transaction trans = _database.TransactionManager.TopTransaction)
            {
                Curve c = (Curve)trans.GetObject(BaseObject, OpenMode.ForRead);
                return c.Closed;
            }
        }
    }
}
