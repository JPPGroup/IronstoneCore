using System;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using NUnit.Framework;

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
    }
}
