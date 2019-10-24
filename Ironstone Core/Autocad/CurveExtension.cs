using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class CurveExtension
    {
        public static double TryGetDistAtPoint(this Curve curve, Point3d point)
        {
            try
            {
                return curve.GetDistAtPoint(point);
            }
            catch (Autodesk.AutoCAD.Runtime.Exception exception)
            {
                if (exception.ErrorStatus == ErrorStatus.InvalidInput) return -1; //Assumed "invalid input" is point not on line. 

                throw;
            }
        }

        public static Curve CreateOffset(this Curve curve, Side side, double dist)
        {
            if (!(dist > 0)) return null;

            return curve switch
            {
                Polyline polyline => CreatePolylineOffset(polyline, side, dist),
                _ => side switch
                {
                    Side.Left => curve.CreateLeftOffset(dist),
                    Side.Right => curve.CreateRightOffset(dist),
                    _ => throw new ArgumentOutOfRangeException(nameof(side), side, @"Invalid side of centre"),
                },
            };
        }

        #region Private methods

        private static Curve CreatePolylineOffset(Polyline polyline, Side side, double offset)
        {
            var index = 0;
            Polyline polylineOffset = null;
            for (var i = 0; i < polyline.NumberOfVertices - 1; i++)
            {
                index = i;
                if (polylineOffset != null) break;

                polylineOffset = polyline.GetOffsetCurveAt(i, side, offset).ToPolyline();
            }

            if (polylineOffset == null) return null;
            var entityList = new List<Entity>();

            for (var i = index; i < polyline.NumberOfVertices - 1; i++)
            {
                var curveOffset = polyline.GetOffsetCurveAt(i, side, offset);
                if (curveOffset == null) throw new ArgumentNullException(nameof(curveOffset), @"Offset curve not set");

                entityList.Add(curveOffset);
            }

            if (entityList.Any())
            {
                var integerCollection = polylineOffset.JoinEntities(entityList.ToArray());
                if (integerCollection.Count != entityList.Count) throw new ArgumentException(@"Incorrect number of entities joined", nameof(entityList));
            }

            return polylineOffset;
        }

        private static double AngleFromCurveToForSide(this Curve curve, Side side)
        {
            double curveAngle;
            switch (curve)
            {
                case Line line:
                    curveAngle = line.Angle;
                    break;
                case Arc arc:
                    var startPoint = new Point2d(arc.StartPoint.X, arc.StartPoint.Y);
                    var arcCentre = new Point2d(arc.Center.X, arc.Center.Y);
                    var startVector = arcCentre.GetVectorTo(startPoint);
                    curveAngle = arc.IsClockwise() ?
                        startVector.Angle - Constants.ANGLE_RADIANS_90_DEGREES :
                        startVector.Angle + Constants.ANGLE_RADIANS_90_DEGREES;
                    break;
                case Polyline polyline:
                    switch (polyline.GetSegmentType(0))
                    {
                        case SegmentType.Line:
                            var lineSegment = polyline.GetLineSegment2dAt(0);

                            curveAngle = lineSegment.Direction.Angle;
                            break;
                        case SegmentType.Arc:
                            var arcSegment = polyline.GetArcSegment2dAt(0);

                            var startSegPoint = new Point2d(arcSegment.StartPoint.X, arcSegment.StartPoint.Y);
                            var arcSegCentre = new Point2d(arcSegment.Center.X, arcSegment.Center.Y);
                            var startSegVector = arcSegCentre.GetVectorTo(startSegPoint);
                            curveAngle = arcSegment.IsClockWise ?
                                startSegVector.Angle - Constants.ANGLE_RADIANS_90_DEGREES :
                                startSegVector.Angle + Constants.ANGLE_RADIANS_90_DEGREES;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(polyline), polyline, @"Initial segment type not handled");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(curve), curve, @"Type not handled");
            }

            return AngleHelper.ForSide(curveAngle, side);
        }

        private static Curve CreateLeftOffset(this Curve curve, double offset)
        {
            Curve offsetObj = null;

            var posLOffset = curve.TryCreateOffset(offset);
            if (curve.IsOffSetValidForSide(posLOffset, Side.Left)) offsetObj = posLOffset;

            var negLOffset = curve.TryCreateOffset(-offset);
            if (curve.IsOffSetValidForSide(negLOffset, Side.Left)) offsetObj = negLOffset;

            return offsetObj;
        }

        private static Curve CreateRightOffset(this Curve curve, double offset)
        {
            Curve offsetObj = null;

            var posROffset = curve.TryCreateOffset(offset);
            if (curve.IsOffSetValidForSide(posROffset, Side.Right)) offsetObj = posROffset;

            var negROffset = curve.TryCreateOffset(-offset);
            if (curve.IsOffSetValidForSide(negROffset, Side.Right)) offsetObj = negROffset;

            return offsetObj;
        }

        private static Curve TryCreateOffset(this Curve curve, double offset)
        {
            try
            {
                return curve.GetOffsetCurves(offset)[0] as Curve;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private static bool IsOffSetValidForSide(this Curve curve, Curve offSet, Side side)
        {
            if (offSet == null) return false;

            var start = new Point2d(curve.StartPoint.X, curve.StartPoint.Y);
            var offSetVector = start.GetVectorTo(new Point2d(offSet.StartPoint.X, offSet.StartPoint.Y));

            return Math.Abs(curve.AngleFromCurveToForSide(side) - offSetVector.Angle) < Constants.ANGLE_TOLERANCE;
        }

        #endregion
    }
}
