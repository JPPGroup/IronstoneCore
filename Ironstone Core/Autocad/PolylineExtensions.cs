using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Helpers;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class PolylineExtensions
    {
        public static int GetIndexAtPoint(this Polyline pLine, Point3d point)
        {
            for (var i = 0; i < pLine.NumberOfVertices - 1; i++)
            {
                using var curve = pLine.GetCurveAt(i);
                if (curve.TryGetDistAtPoint(point) > 0) return i;
            }

            return -1;
        }

        public static Curve GetCurveAt(this Polyline pLine, int index)
        {
            switch (pLine.GetSegmentType(index))
            {
                case SegmentType.Arc:
                    var arc2d = pLine.GetArcSegment2dAt(index);

                    return GetArcAt(pLine, arc2d);
                case SegmentType.Line:
                    var line2d = pLine.GetLineSegment2dAt(index);

                    return GetLineAt(pLine, line2d);
                default:
                    throw new ArgumentOutOfRangeException(nameof(pLine), pLine, @"Segment type not handled");
            }
        }

        public static bool IntersectWithSelf(this Polyline pLine, Point3dCollection points)
        {
            var entities = new DBObjectCollection();
            pLine.Explode(entities);

            for (var i = 0; i < entities.Count; ++i)
            {
                for (var j = i + 1; j < entities.Count; ++j)
                {
                    var curve1 = (Curve)entities[i];
                    var curve2 = (Curve)entities[j];
                    var pts = new Point3dCollection();

                    curve1.IntersectWith(curve2, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                    foreach (Point3d point in pts)
                    {
                        // Make a check to skip the start/end points since they are connected vertices
                        if (point == curve1.StartPoint || point == curve1.EndPoint)
                        {
                            if (point == curve2.StartPoint || point == curve2.EndPoint)
                            {
                                // If two consecutive segments, then skip
                                if (j == i + 1) continue;
                            }
                        }

                        points.Add(point);
                    }
                }

                entities[i].Dispose();
            }

            return points.Count > 0;
        }

        public static Polyline GetSectionBetween(this Polyline pLine, Point3d startPoint, Point3d endPoint)
        {
            if (pLine.StartPoint.IsEqualTo(startPoint) && pLine.EndPoint.IsEqualTo(endPoint)) return pLine;

            var points = new Point3dCollection();
            if (!pLine.StartPoint.IsEqualTo(startPoint)) points.Add(startPoint);
            if (!pLine.EndPoint.IsEqualTo(endPoint)) points.Add(endPoint);

            var splits = pLine.GetSplitCurves(points);

            foreach (Polyline split in splits)
            {
                if (split.StartPoint.IsEqualTo(startPoint) && split.EndPoint.IsEqualTo(endPoint)) return split;
            }

            throw new ArgumentException(@"Section of polyline could not be created", nameof(pLine));
        }

        public static Curve GetOffsetCurveAt(this Polyline pLine, int index, Side side, double dist)
        {
            var sideReverse = side == Side.Left ? Side.Right : Side.Left;

            switch (pLine.GetSegmentType(index))
            {
                case SegmentType.Arc:
                    var arc2d = pLine.GetArcSegment2dAt(index);
                    using (var arc = GetArcAt(pLine, arc2d))
                    {
                        return arc2d.IsClockWise
                            ? arc.CreateOffset(sideReverse, dist)
                            : arc.CreateOffset(side, dist);
                    }
                case SegmentType.Line:
                    var line2d = pLine.GetLineSegment2dAt(index);
                    using (var line = GetLineAt(pLine, line2d))
                    {
                        return line.CreateOffset(side, dist);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(pLine), pLine, @"Segment type not handled");
            }
        }

        public static double GetTangentialAngleAtStart(this Polyline pLine)
        {
            const int startIndex = 0;

            switch (pLine.GetSegmentType(startIndex))
            {
                case SegmentType.Arc:
                    var arc2d = pLine.GetArcSegment2dAt(startIndex);
                    var angle = arc2d.Center.GetVectorTo(arc2d.StartPoint).Angle;
                    return arc2d.IsClockWise
                        ? AngleHelper.ForRightSide(angle)
                        : AngleHelper.ForLeftSide(angle);
                case SegmentType.Line:
                    var line2d = pLine.GetLineSegment2dAt(startIndex);
                    return line2d.StartPoint.GetVectorTo(line2d.EndPoint).Angle;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pLine), pLine, @"Segment type not handled");
            }
        }

        public static double GetTangentialAngleAtEnd(this Polyline pLine)
        {
            var endIndex = pLine.NumberOfVertices - 2;

            switch (pLine.GetSegmentType(endIndex))
            {
                case SegmentType.Arc:
                    var arc2d = pLine.GetArcSegment2dAt(endIndex);
                    var angle = arc2d.Center.GetVectorTo(arc2d.EndPoint).Angle;
                    return arc2d.IsClockWise
                        ? AngleHelper.ForLeftSide(angle)
                        : AngleHelper.ForRightSide(angle);
                case SegmentType.Line:
                    var line2d = pLine.GetLineSegment2dAt(endIndex);
                    return line2d.EndPoint.GetVectorTo(line2d.StartPoint).Angle;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pLine), pLine, @"Segment type not handled");
            }
        }


        public static void FilletAll(this Polyline pLine)
        {
            var j = pLine.Closed ? 0 : 1;
            for (var i = j; i < pLine.NumberOfVertices - j; i++)
            {
                var radius = CalcRadius(pLine, i);
                i += pLine.FilletAt(i, radius);
            }
        }

        public static int FilletAt(this Polyline pLine, int index, double radius)
        {
            var prev = index == 0 && pLine.Closed ? pLine.NumberOfVertices - 1 : index - 1;
            if (pLine.GetSegmentType(prev) != SegmentType.Line || pLine.GetSegmentType(index) != SegmentType.Line) return 0;

            var seg1 = pLine.GetLineSegment2dAt(prev);
            var seg2 = pLine.GetLineSegment2dAt(index);
            var vec1 = seg1.StartPoint - seg1.EndPoint;
            var vec2 = seg2.EndPoint - seg2.StartPoint;
            var angle = vec1.GetAngleTo(vec2) / 2.0;
            var dist = radius / Math.Tan(angle);

            if (dist <= 0 || dist > seg1.Length || dist > seg2.Length) return 0;

            var pt1 = seg1.EndPoint + vec1.GetNormal() * dist;
            var pt2 = seg2.StartPoint + vec2.GetNormal() * dist;
            var bulge = Math.Tan((Math.PI / 2.0 - angle) / 2.0);
            if (Clockwise(seg1.StartPoint, seg1.EndPoint, seg2.EndPoint)) bulge = -bulge;

            pLine.AddVertexAt(index, pt1, bulge, 0.0, 0.0);
            pLine.SetPointAt(index + 1, pt2);
            return 1;
        }

        #region Private methods

        private static bool Clockwise(Point2d p1, Point2d p2, Point2d p3)
        {
            return ((p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X)) < 1e-8;
        }

        private static double CalcRadius(Polyline pLine, int index)
        {
            var prev = index == 0 && pLine.Closed ? pLine.NumberOfVertices - 1 : index - 1;
            if (pLine.GetSegmentType(prev) != SegmentType.Line || pLine.GetSegmentType(index) != SegmentType.Line) throw new ArgumentException("Cannot calculation radius, for fillet on polyline.");

            var seg1 = pLine.GetLineSegment2dAt(prev);
            var seg2 = pLine.GetLineSegment2dAt(index);
            var vec1 = seg1.StartPoint - seg1.EndPoint;
            var vec2 = seg2.EndPoint - seg2.StartPoint;
            var angle = vec1.GetAngleTo(vec2);

            var radius = angle < Math.PI / 2 ? 100 : angle < Math.PI ? 200 : 500;

            var dist = radius / Math.Tan(angle / 2.0);
            if (dist <= 0 || dist > seg1.Length || dist > seg2.Length) radius /= 2;

            return radius;
        }

        private static Line GetLineAt(Polyline pLine, LineSegment2d line2d)
        {
            var line2dInterval = line2d.GetInterval();

            var startLineParam = line2dInterval.LowerBound;
            var endLineParam = line2dInterval.UpperBound;

            var spLine2d = line2d.EvaluatePoint(startLineParam);
            var epLine2d = line2d.EvaluatePoint(endLineParam);

            var sp3d = new Point3d(spLine2d.X, spLine2d.Y, pLine.Elevation);
            var ep3d = new Point3d(epLine2d.X, epLine2d.Y, pLine.Elevation);

            if (pLine.Normal != Vector3d.ZAxis)
            {
                var ecsMatrix = pLine.Ecs;
                sp3d = sp3d.TransformBy(ecsMatrix);
                ep3d = ep3d.TransformBy(ecsMatrix);
            }

            return new Line(sp3d, ep3d);
        }

        private static Arc GetArcAt(Polyline pLine, CircularArc2d arc2d)
        {
            var arc2dInterval = arc2d.GetInterval();

            var startArcParam = arc2dInterval.LowerBound;
            var endArcParam = arc2dInterval.UpperBound;

            var spArc2d = arc2d.EvaluatePoint(startArcParam);
            var epArc2d = arc2d.EvaluatePoint(endArcParam);
            var cpArc2d = arc2d.Center;

            using var startLine = new Line2d(cpArc2d, spArc2d);
            using var endLine = new Line2d(cpArc2d, epArc2d);

            var startAngle = startLine.Direction.Angle;
            var endAngle = endLine.Direction.Angle;

            var cp3d = new Point3d(cpArc2d.X, cpArc2d.Y, pLine.Elevation);

            if (pLine.Normal != Vector3d.ZAxis)
            {
                var ecsMatrix = pLine.Ecs;
                cp3d = cp3d.TransformBy(ecsMatrix);
            }

            return arc2d.IsClockWise
                ? new Arc(cp3d, pLine.Normal, arc2d.Radius, endAngle, startAngle)
                : new Arc(cp3d, pLine.Normal, arc2d.Radius, startAngle, endAngle);
        }

        #endregion
    }
}
