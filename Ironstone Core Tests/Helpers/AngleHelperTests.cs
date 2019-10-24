using Jpp.Ironstone.Core.Helpers;
using NUnit.Framework;
using System;
using System.Collections;

namespace Jpp.Ironstone.Core.Tests.Helpers
{
    [TestFixture]
    public class AngleHelperTests
    {
        [TestCaseSource(typeof(AngleHelperData), nameof(AngleHelperData.ForSideLeftTestCases))]
        public double VerifyForSide_Left(double angle)
        {
            return AngleHelper.ForSide(angle, Side.Left);
        }

        [TestCaseSource(typeof(AngleHelperData), nameof(AngleHelperData.ForSideRightTestCases))]
        public double VerifyForSide_Right(double angle)
        {
            return AngleHelper.ForSide(angle, Side.Right);
        }

        [Test]
        public void VerifyForSide_Throw_Exception_Test()
        {
            const Side side = (Side)(-1);

            Assert.Throws<ArgumentOutOfRangeException>(() => AngleHelper.ForSide(0, side));
        }

        [TestCaseSource(typeof(AngleHelperData), nameof(AngleHelperData.ForSideRightTestCases))]
        public double VerifyForRightSide(double angle)
        {
            return AngleHelper.ForRightSide(angle);
        }

        [TestCaseSource(typeof(AngleHelperData), nameof(AngleHelperData.ForSideLeftTestCases))]
        public double VerifyForLeftSide(double angle)
        {
            return AngleHelper.ForLeftSide(angle);
        }

        private static class AngleHelperData
        {
            public static IEnumerable ForSideRightTestCases
            {
                get
                {
                    yield return new TestCaseData(Math.PI).Returns(Math.PI - (Math.PI / 2));
                    yield return new TestCaseData(2 * Math.PI).Returns((2 * Math.PI) - (Math.PI / 2));
                    yield return new TestCaseData(Math.PI / 2).Returns(0);
                    yield return new TestCaseData(Math.PI / 4).Returns(Math.PI * 1.75);
                }
            }

            public static IEnumerable ForSideLeftTestCases
            {
                get
                {
                    yield return new TestCaseData(Math.PI).Returns(Math.PI + (Math.PI / 2));
                    yield return new TestCaseData(2 * Math.PI).Returns(Math.PI / 2);
                    yield return new TestCaseData(Math.PI / 2).Returns(Math.PI);
                    yield return new TestCaseData(Math.PI * 1.5).Returns(0);
                }
            }
        }
    }
}
