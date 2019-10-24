using Autodesk.AutoCAD.Geometry;
using System;
using System.Xml.Serialization;

namespace Jpp.Ironstone.Core.Autocad
{
    [Serializable]
    public class SerializablePoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        [XmlIgnore]
        public Point3d Point3d
        {
            get => new Point3d(X, Y, Z);
            set
            {
                if (value.IsEqualTo(Point3d)) return;

                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        [XmlIgnore]
        public Point2d Point2d
        {
            get => new Point2d(X, Y);
            set
            {
                if (value.IsEqualTo(Point2d)) return;

                X = value.X;
                Y = value.Y;
                Z = 0; //TODO: Review if this is appropriate
            }
        }
    }
}
