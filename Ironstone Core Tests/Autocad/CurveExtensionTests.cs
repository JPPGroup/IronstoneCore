using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Tests.SerializableObjects;
using NUnit.Framework;
using System;
using System.Collections;
using System.Reflection;

namespace Jpp.Ironstone.Core.Tests.Autocad
{
    public class CurveExtensionTests : IronstoneTestFixture
    {
        public CurveExtensionTests() : base(Assembly.GetExecutingAssembly(), typeof(CurveExtensionTests)) { }

        [TestCaseSource(typeof(CurveExtensionData), nameof(CurveExtensionData.LineDistAtPointTestCases))]
        public double VerifyLineTryGetDistAtPoint(double x, double y, double z)
        {
            var point = new Coordinates3d { X = x, Y = y, Z = z };
            var result = RunTest<double>(nameof(VerifyLineTryGetDistAtPointResident), point);
            return result;
        }

        [Test]
        public void VerifyCreateOffset_Throw_Exception()
        {
            var result = RunTest<bool>(nameof(VerifyCreateOffset_Throw_ExceptionResident));
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyCreateOffsetZeroDistance()
        {
            var result = RunTest<bool>(nameof(VerifyCreateOffsetZeroDistanceResident));
            Assert.IsTrue(result);
        }

        public static bool VerifyCreateOffset_Throw_ExceptionResident()
        {
            try
            {
                const Side side = (Side)(-1);
                using var line = new Line();
                {
                    var _ = line.CreateOffset(side, 1);
                }
                
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool VerifyCreateOffsetZeroDistanceResident()
        {
            try
            {
                const Side side = (Side)(-1);
                using var line = new Line();
                {
                    var result = line.CreateOffset(side, 0);
                    return result is null;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static double VerifyLineTryGetDistAtPointResident(Coordinates3d coordinates)
        {
            var line = new Line(new Point3d(0, 0,0), new Point3d(0,10,0));

            return line.TryGetDistAtPoint(new Point3d(coordinates.X, coordinates.Y, coordinates.Z));
        }


        private static class CurveExtensionData
        {
            public static IEnumerable LineDistAtPointTestCases
            {
                get
                {
                    yield return new TestCaseData(0, 0, 0).Returns(0);
                    yield return new TestCaseData(0, 1, 0).Returns(1);
                    yield return new TestCaseData(0, 2, 0).Returns(2);
                    yield return new TestCaseData(0, 3, 0).Returns(3);
                    yield return new TestCaseData(0, 4, 0).Returns(4);
                    yield return new TestCaseData(0, 5, 0).Returns(5);
                    yield return new TestCaseData(0, 6, 0).Returns(6);
                    yield return new TestCaseData(0, 7, 0).Returns(7);
                    yield return new TestCaseData(0, 8, 0).Returns(8);
                    yield return new TestCaseData(0, 9, 0).Returns(9);
                    yield return new TestCaseData(0, 10, 0).Returns(10);
                    yield return new TestCaseData(0, 11, 0).Returns(-1);
                    yield return new TestCaseData(1, 1, 0).Returns(-1);
                    yield return new TestCaseData(0, 1, 1).Returns(-1);
                    yield return new TestCaseData(1, 1, 1).Returns(-1);
                }
            }
        }
    }
}
