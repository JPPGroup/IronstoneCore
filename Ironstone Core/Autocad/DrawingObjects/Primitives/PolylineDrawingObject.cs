using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Transaction = Autodesk.AutoCAD.DatabaseServices.Transaction;

namespace Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives
{
    /// <summary>
    /// This class is only present currently for the inheritance chain
    /// TODO: Flesh out polyline
    /// TODO: Override setter for baseobject to check if actually a polyline
    /// </summary>
    public class PolylineDrawingObject : CurveDrawingObject
    {
        protected PolylineDrawingObject()
        {

        }

        public PolylineDrawingObject(Polyline polyline)
        {
            BaseObject = polyline.Id;
        }

        public PolylineDrawingObject(Polyline2d polyline)
        {
            BaseObject = polyline.Id;
        }

        public PolylineDrawingObject(Polyline3d polyline)
        {
            BaseObject = polyline.Id;
        }

        protected override void ObjectModified(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override Point3d Location { get; set; }
        public override void Generate()
        {
            throw new NotImplementedException();
        }

        public override double Rotation { get; set; }
        public override void Erase()
        {
            throw new NotImplementedException();
        }

        
    }
}
