using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.Autocad
{
    public class BlockDrawingObject : DrawingObject
    {
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
