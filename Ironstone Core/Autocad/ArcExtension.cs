using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

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
    }
}
