using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Tests.SerializableObjects;
using NUnit.Framework;
using System.Collections;
using System.Reflection;

namespace Jpp.Ironstone.Core.Tests.Autocad
{
    [TestFixture]
    public class SerializablePointTests : IronstoneTestFixture
    {
        public SerializablePointTests() : base(Assembly.GetExecutingAssembly(), typeof(SerializablePointTests)) { }

        [TestCaseSource(typeof(SerializablePointData), nameof(SerializablePointData.Coordinates3dTestCases))]
        public void VerifyGetPoint3dFromSetX_Y_Z(double x, double y, double z)
        {
            var values = new Coordinates3d { X = x, Y = y, Z = z };
            var result = RunTest<Coordinates3d>(nameof(VerifyGetPoint3dFromSetX_Y_ZResident), values);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(x, result.X, "X value is incorrect");
                Assert.AreEqual(y, result.Y, "Y value is incorrect");
                Assert.AreEqual(z, result.Z, "Z value is incorrect");
            });
        }

        [TestCaseSource(typeof(SerializablePointData), nameof(SerializablePointData.Coordinates3dTestCases))]
        public void VerifyGetPoint2dFromSetX_Y_Z(double x, double y, double z)
        {
            var values = new Coordinates3d { X = x, Y = y, Z = z };
            var result = RunTest<Coordinates2d>(nameof(VerifyGetPoint2dFromSetX_Y_ZResident), values);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(x, result.X, "X value is incorrect");
                Assert.AreEqual(y, result.Y, "Y value is incorrect");
            });
        }

        [TestCaseSource(typeof(SerializablePointData), nameof(SerializablePointData.Coordinates3dTestCases))]
        public void VerifyGetX_Y_ZFromSetPoint3d(double x, double y, double z)
        {
            var values = new Coordinates3d { X = x, Y = y, Z = z };
            var result = RunTest<Coordinates3d>(nameof(VerifyGetX_Y_ZFromSetPoint3dResident), values);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(x, result.X, "X value is incorrect");
                Assert.AreEqual(y, result.Y, "Y value is incorrect");
                Assert.AreEqual(z, result.Z, "Z value is incorrect");
            });
        }

        [TestCaseSource(typeof(SerializablePointData), nameof(SerializablePointData.Coordinates3dTestCases))]
        public void VerifyGetPoint2dFromSetPoint3d(double x, double y, double z)
        {
            var values = new Coordinates3d { X = x, Y = y, Z = z };
            var result = RunTest<Coordinates2d>(nameof(VerifyGetPoint2dFromSetPoint3dResident), values);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(x, result.X, "X value is incorrect");
                Assert.AreEqual(y, result.Y, "Y value is incorrect");
            });
        }

        [TestCaseSource(typeof(SerializablePointData), nameof(SerializablePointData.Coordinates2dTestCases))]
        public void VerifyGetX_Y_ZFromSetPoint2d(double x, double y)
        {
            var values = new Coordinates2d { X = x, Y = y };
            var result = RunTest<Coordinates3d>(nameof(VerifyGetX_Y_ZFromSetPoint2dResident), values);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(x, result.X, "X value is incorrect");
                Assert.AreEqual(y, result.Y, "Y value is incorrect");
                Assert.AreEqual(0, result.Z, "Z value is incorrect");
            });
        }

        [TestCaseSource(typeof(SerializablePointData), nameof(SerializablePointData.Coordinates2dTestCases))]
        public void VerifyGetPoint3dFromSetPoint2d(double x, double y)
        {
            var values = new Coordinates2d { X = x, Y = y };
            var result = RunTest<Coordinates3d>(nameof(VerifyGetPoint3dFromSetPoint2dResident), values);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(x, result.X, "X value is incorrect");
                Assert.AreEqual(y, result.Y, "Y value is incorrect");
                Assert.AreEqual(0, result.Z, "Z value is incorrect");
            });
        }

        public static Coordinates3d VerifyGetPoint3dFromSetX_Y_ZResident(Coordinates3d coordinates)
        {
            var point = new SerializablePoint { X = coordinates.X, Y = coordinates.Y, Z = coordinates.Z };
            var result = new Coordinates3d
            {
                X = point.Point3d.X,
                Y = point.Point3d.Y,
                Z = point.Point3d.Z
            };

            return result;
        }

        public static Coordinates2d VerifyGetPoint2dFromSetX_Y_ZResident(Coordinates3d coordinates)
        {
            var point = new SerializablePoint { X = coordinates.X, Y = coordinates.Y, Z = coordinates.Z };
            var result = new Coordinates2d
            {
                X = point.Point2d.X,
                Y = point.Point2d.Y
            };

            return result;
        }

        public static Coordinates3d VerifyGetX_Y_ZFromSetPoint3dResident(Coordinates3d coordinates)
        {
            var point3d = new Point3d(coordinates.X, coordinates.Y, coordinates.Z);
            var point = new SerializablePoint { Point3d = point3d };
            var result = new Coordinates3d
            {
                X = point.X,
                Y = point.Y,
                Z = point.Z
            };

            return result;
        }

        public static Coordinates2d VerifyGetPoint2dFromSetPoint3dResident(Coordinates3d coordinates)
        {
            var point3d = new Point3d(coordinates.X, coordinates.Y, coordinates.Z);
            var point = new SerializablePoint { Point3d = point3d };
            var result = new Coordinates2d
            {
                X = point.Point2d.X,
                Y = point.Point2d.Y
            };

            return result;
        }

        public static Coordinates3d VerifyGetX_Y_ZFromSetPoint2dResident(Coordinates2d coordinates)
        {
            var point2d = new Point2d(coordinates.X, coordinates.Y);
            var point = new SerializablePoint { Point2d = point2d };
            var result = new Coordinates3d
            {
                X = point.X,
                Y = point.Y,
                Z = point.Z
            };

            return result;
        }

        public static Coordinates3d VerifyGetPoint3dFromSetPoint2dResident(Coordinates2d coordinates)
        {
            var point2d = new Point2d(coordinates.X, coordinates.Y);
            var point = new SerializablePoint { Point2d = point2d };
            var result = new Coordinates3d
            {
                X = point.Point3d.X,
                Y = point.Point3d.Y,
                Z = point.Point3d.Z
            };

            return result;
        }

        private static class SerializablePointData
        {
            public static IEnumerable Coordinates3dTestCases
            {
                get
                {
                    yield return new TestCaseData(0, 0, 0);

                    yield return new TestCaseData(double.MinValue, 0, 0);
                    yield return new TestCaseData(0, double.MinValue, 0);
                    yield return new TestCaseData(0, 0, double.MinValue);
                    yield return new TestCaseData(double.MinValue, double.MinValue, 0);
                    yield return new TestCaseData(double.MinValue, 0, double.MinValue);
                    yield return new TestCaseData(0, double.MinValue, double.MinValue);
                    yield return new TestCaseData(double.MinValue, double.MinValue, double.MinValue);

                    yield return new TestCaseData(double.MaxValue, 0, 0);
                    yield return new TestCaseData(0, double.MaxValue, 0);
                    yield return new TestCaseData(0, 0, double.MaxValue);
                    yield return new TestCaseData(double.MaxValue, double.MaxValue, 0);
                    yield return new TestCaseData(double.MaxValue, 0, double.MaxValue);
                    yield return new TestCaseData(0, double.MaxValue, double.MaxValue);
                    yield return new TestCaseData(double.MaxValue, double.MaxValue, double.MaxValue);
                }
            }

            public static IEnumerable Coordinates2dTestCases
            {
                get
                {
                    yield return new TestCaseData(0, 0);

                    yield return new TestCaseData(double.MinValue, 0);
                    yield return new TestCaseData(0, double.MinValue);
                    yield return new TestCaseData(double.MinValue, double.MinValue);
                    
                    yield return new TestCaseData(double.MaxValue, 0);
                    yield return new TestCaseData(0, double.MaxValue);
                    yield return new TestCaseData(double.MaxValue, double.MaxValue);
                }
            }
        }
    }
}
