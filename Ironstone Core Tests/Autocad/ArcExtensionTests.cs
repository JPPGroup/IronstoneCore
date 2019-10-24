using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using NUnit.Framework;
using System;
using System.Collections;
using System.Reflection;

namespace Jpp.Ironstone.Core.Tests.Autocad
{
    [TestFixture]
    public class ArcExtensionTests : IronstoneTestFixture
    {
        public ArcExtensionTests() : base(Assembly.GetExecutingAssembly(), typeof(ArcExtensionTests)) { }

        [Test]
        public void VerifyAntiClockwiseArc()
        {
            var result = RunTest<bool>(nameof(VerifyAntiClockwiseArcResident));
            Assert.IsFalse(result, "Arc is not anti clockwise.");
        }

        public bool VerifyAntiClockwiseArcResident()
        {
            const int radius = 6;
            const double startAngle = 0;
            const double endAngle = Math.PI;

            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);

            return arc.IsClockwise();
        }

        [Test]
        public void VerifyAntiClockwiseArcReverseCurve()
        {
            var result = RunTest<bool>(nameof(VerifyAntiClockwiseArcReverseCurveResident));
            Assert.IsTrue(result, "Arc is not clockwise.");
        }

        public bool VerifyAntiClockwiseArcReverseCurveResident()
        {
            const int radius = 6;
            const double startAngle = 0;
            const double endAngle = Math.PI;

            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);
            arc.ReverseCurve();

            return arc.IsClockwise();
        }

        [Test]
        public void VerifyClockwiseArc()
        {
            var result = RunTest<bool>(nameof(VerifyClockwiseArcResident));
            Assert.IsTrue(result, "Arc is not clockwise.");
        }

        public bool VerifyClockwiseArcResident()
        {
            const int radius = 6;
            const double startAngle = Math.PI;
            const double endAngle = 0;

            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);

            return arc.IsClockwise();
        }

        [Test]
        public void VerifyClockwiseArcReverseCurve()
        {
            var result = RunTest<bool>(nameof(VerifyClockwiseArcReverseCurveResident));
            Assert.IsFalse(result, "Arc is not anti-clockwise.");
        }

        public bool VerifyClockwiseArcReverseCurveResident()
        {
            const int radius = 6;
            const double startAngle = Math.PI;
            const double endAngle = 0;

            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);
            arc.ReverseCurve();

            return arc.IsClockwise();
        }


        [TestCaseSource(typeof(ArcExtensionData), nameof(ArcExtensionData.BulgeTestCases))]
        public double VerifyBulge(double radius, double startAngle, double endAngle)
        {
            var values = new object[] { radius , startAngle, endAngle };
            return RunTest<double>(nameof(VerifyBulgeResident), values);
        }

        public static double VerifyBulgeResident(object[] values)
        {
            var radius = (double) values[0];
            var startAngle = (double)values[1];
            var endAngle = (double)values[2];
            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);
            return arc.Bulge(new Plane(Point3d.Origin, Vector3d.ZAxis));
        }

        private static class ArcExtensionData
        {
            public static IEnumerable BulgeTestCases
            {
                get
                {
                    yield return new TestCaseData(5, 0, Math.PI / 4).Returns(0.19891236737965792d);
                    yield return new TestCaseData(5, 0, Math.PI / 2).Returns(0.41421356237309515d);
                    yield return new TestCaseData(5, 0, Math.PI).Returns(1.0d);
                    yield return new TestCaseData(5, 0, Math.PI * 1.5).Returns(0.41421356237309509d);
                    yield return new TestCaseData(5, 0, Math.PI * 2).Returns(double.NaN);

                    yield return new TestCaseData(10, 0, Math.PI / 4).Returns(0.19891236737965792d);
                    yield return new TestCaseData(10, 0, Math.PI / 2).Returns(0.41421356237309515d);
                    yield return new TestCaseData(10, 0, Math.PI).Returns(1.0d);
                    yield return new TestCaseData(10, 0, Math.PI * 1.5).Returns(0.41421356237309509d);
                    yield return new TestCaseData(10, 0, Math.PI * 2).Returns(double.NaN);

                    yield return new TestCaseData(20, 0, Math.PI / 4).Returns(0.19891236737965792d);
                    yield return new TestCaseData(20, 0, Math.PI / 2).Returns(0.41421356237309515d);
                    yield return new TestCaseData(20, 0, Math.PI).Returns(1.0d);
                    yield return new TestCaseData(20, 0, Math.PI * 1.5).Returns(0.41421356237309509d);
                    yield return new TestCaseData(20, 0, Math.PI * 2).Returns(double.NaN);
                }
            }
        }
    }
}
