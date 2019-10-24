using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class ArcExtension
    {
        public static bool IsClockwise(this Arc arc)
        {
            var startPoint = new Point2d(arc.StartPoint.X, arc.StartPoint.Y);
            var centrePoint = new Point2d(arc.Center.X, arc.Center.Y);
            var endPoint = new Point2d(arc.EndPoint.X, arc.EndPoint.Y);
            var vecCentreStart = centrePoint.GetVectorTo(startPoint);
            var vecCentreEnd = centrePoint.GetVectorTo(endPoint);

            return vecCentreStart.X * vecCentreEnd.Y - vecCentreStart.Y * vecCentreEnd.X < 0;
        }

        public static double Bulge(this Arc arc, Plane plane)
        {
            var arcPt1 = arc.StartPoint.Convert2d(plane);
            var arcPt2 = arc.EndPoint.Convert2d(plane);

            var vec = arcPt1 - arcPt2;

            var halfChord = vec.Length * 0.5;
            var radius = arc.Radius;
            var radiusSq = Math.Pow(radius, 2);
            var halfChordSq = Math.Pow(halfChord, 2);
            var sagitta = radius - Math.Sqrt(radiusSq - halfChordSq);

            return sagitta / halfChord;
        }

    }
}
