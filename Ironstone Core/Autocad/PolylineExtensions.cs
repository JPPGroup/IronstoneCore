using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class PolylineExtensions
    {
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

        public static DBObjectCollection ExplodeAndErase(this Polyline pLine)
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var acDbObjColl = new DBObjectCollection();

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var actualPolyline = acTrans.GetObject(pLine.ObjectId, OpenMode.ForWrite) as Polyline;
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (acBlkTbl != null)
                {
                    var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    if (acBlkTblRec != null)
                    {
                        actualPolyline?.Explode(acDbObjColl);

                        foreach (Entity acEnt in acDbObjColl)
                        {
                            acBlkTblRec.AppendEntity(acEnt);
                            acTrans.AddNewlyCreatedDBObject(acEnt, true);
                        }

                        actualPolyline?.Erase();
                    }
                }
                acTrans.Commit();
            }

            return acDbObjColl;
        }


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
            if (dist <= 0 || dist > seg1.Length || dist > seg2.Length) radius = radius / 2;

            return radius;
        }
    }
}
