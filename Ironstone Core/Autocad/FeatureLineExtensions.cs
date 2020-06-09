using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil;
using Autodesk.Civil.DatabaseServices;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class FeatureLineExtensions
    {
        public static double GetElevationAtPoint(this FeatureLine featureLine, Point3d point)
        {
            Point3dCollection points = featureLine.GetPoints(FeatureLinePointType.AllPoints);

            //Se if point already exists
            foreach (Point3d point3d in points)
            {
                if (point3d.X == point.X && point3d.Y == point.Y)
                {
                    return point3d.Z;
                }
            }

            double targetParam = featureLine.GetParameterAtPoint(point);
            Point3d lastPoint = Point3d.Origin;

            //if not find closest point and interpolate between
            for (int i = 0; i < points.Count; i++)
            {
                Point3d testPoint = points[i];
                double param = featureLine.GetParameterAtPoint(testPoint);
                if(targetParam < param)
                    break;

                lastPoint = testPoint;
            }

            double additionalDistance = Math.Sqrt(Math.Pow(point.X - lastPoint.X, 2) + Math.Pow(point.Y - lastPoint.Y, 2));
            double grade = featureLine.GetGradeOutAtPoint(lastPoint);
            return (lastPoint.Z + grade * additionalDistance);
        }
    }
}
