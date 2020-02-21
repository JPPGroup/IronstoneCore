using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.Tests.TestObjects
{
    class TestBlockRefDrawingObject : BlockRefDrawingObject
    {
        public TestBlockRefDrawingObject(BlockReference br) : base(br)
        { }

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
