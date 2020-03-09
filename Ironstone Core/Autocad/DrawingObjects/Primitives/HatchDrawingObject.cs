using System;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives
{
    public class HatchDrawingObject : DrawingObject
    {
        public HatchDrawingObject(Hatch hatch)
        {
            BaseObject = hatch.ObjectId;
        }

        protected HatchDrawingObject()
        { }

        protected override void ObjectModified(object sender, EventArgs e)
        {
            
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            
        }

        public override Point3d Location { get; set; }
        public override void Generate()
        {
            
        }

        public override double Rotation { get; set; }
        public override void Erase()
        {
            
        }

        public Color Color
        {
            get
            {
                Transaction trans = _database.TransactionManager.TopTransaction;
                Hatch hatch = trans.GetObject(BaseObject, OpenMode.ForRead) as Hatch;
                return hatch.Color;
            }
            set
            {
                Transaction trans = _database.TransactionManager.TopTransaction;
                Hatch hatch = trans.GetObject(BaseObject, OpenMode.ForWrite) as Hatch;
                hatch.Color = value;
            }
        }
    }
}
