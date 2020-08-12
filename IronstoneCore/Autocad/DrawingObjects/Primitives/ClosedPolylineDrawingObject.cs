using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives
{
    public abstract class ClosedPolylineDrawingObject : PolylineDrawingObject
    {
        public ClosedPolylineDrawingObject(Polyline polyline) : base(polyline)
        {
        }

        public ClosedPolylineDrawingObject(Polyline2d polyline) : base(polyline)
        {
        }

        public ClosedPolylineDrawingObject(Polyline3d polyline) : base(polyline)
        {
        }

        public ClosedPolylineDrawingObject(PolylineDrawingObject polyline)
        {
            BaseObject = polyline.BaseObject;
        }

        protected ClosedPolylineDrawingObject() : base()
        {

        }

        protected override bool VerifyBaseObject()
        {
            return this.IsClosed();
        }
    }
}
