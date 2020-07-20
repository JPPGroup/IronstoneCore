using System;
using System.Diagnostics;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.Autocad.DrawingObjects.Primitives
{
    class LineDrawingObjectTests : IronstoneTestFixture
    {
        public LineDrawingObjectTests() : base(Assembly.GetExecutingAssembly(), typeof(LineDrawingObjectTests))
        {
        }
        
        [TestCase(50, 50, ExpectedResult = true)]
        [TestCase(100, 100, ExpectedResult = true)]
        [TestCase(0, 0, ExpectedResult = true)]
        [TestCase(500, 500, ExpectedResult = false)]
        [TestCase(-10, -10, ExpectedResult = false)]
        [TestCase(-150, 10, ExpectedResult = false)]
        public bool VerifyIsPointOnLine(double x, double y)
        {
            Point testPoint = new Point() {X = x, Y = y};
            return RunTest<bool>(nameof(VerifyIsPointOnLineResident), testPoint);
        }

        public bool VerifyIsPointOnLineResident(Point testPoint)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LineDrawingObject line1 = LineDrawingObject.Create(doc.Database, Point3d.Origin, new Point3d(100, 100, 0));
                Point2d point = new Point2d(testPoint.X, testPoint.Y);
                return line1.IsPointOnLine(point);
            }
        }

        //TODO: Add more tests, dont think all cases are covered
        [TestCase(25, 25, 50, 50, ExpectedResult = true)]
        [TestCase(50, 50, 0, 0, ExpectedResult = true)] //Verify inverse gradient is treated the same
        [TestCase(125, 125, 150, 150, ExpectedResult = false)]
        [TestCase(100, 100, 200, 200, ExpectedResult = false)] //Check not a continuation
        [TestCase(-100, -100, 0, 0, ExpectedResult = false)] //Check not before
        public bool VerifyIsTargetSegmentOf(double startX, double startY, double endX, double endY)
        {
            Point start = new Point() {X = startX, Y = startY};
            Point end = new Point() {X = endX, Y = endY};
            Line line = new Line() {Start = start, End = end};
            return RunTest<bool>(nameof(VerifyIsTargetSegmentOfResident), line);
        }

        //TODO: Add more tests, dont think all cases are covered
        [TestCase(0, 0, 0, 50, ExpectedResult = true)]
        [TestCase(0, 150, 0, 250, ExpectedResult = false)]
        public bool VerifyIsTargetSegmentOfInfinte(double startX, double startY, double endX, double endY)
        {
            Point start = new Point() {X = startX, Y = startY};
            Point end = new Point() {X = endX, Y = endY};
            Line line = new Line() {Start = start, End = end};
            return RunTest<bool>(nameof(VerifyIsTargetSegmentOfInfiniteResident), line);
        }

        public bool VerifyIsTargetSegmentOfResident(Line line)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                //Debugger.Launch();
                LineDrawingObject subject = LineDrawingObject.Create(doc.Database, Point3d.Origin, new Point3d(100, 100, 0));
                LineDrawingObject target = LineDrawingObject.Create(doc.Database, new Point3d(line.Start.X, line.Start.Y, 0), new Point3d(line.End.X, line.End.Y, 0));

                return subject.IsTargetSegmentOf(target);
            }
        }

        public bool VerifyIsTargetSegmentOfInfiniteResident(Line line)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LineDrawingObject subject = LineDrawingObject.Create(doc.Database, Point3d.Origin, new Point3d(0, 100, 0));
                LineDrawingObject target = LineDrawingObject.Create(doc.Database, new Point3d(line.Start.X, line.Start.Y, 0), new Point3d(line.End.X, line.End.Y, 0));

                return subject.IsTargetSegmentOf(target);
            }
        }

        [Serializable]
        public struct Point
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        [Serializable]
        public struct Line
        {
            public Point Start { get; set; }
            public Point End { get; set; }
        }
    }
}

