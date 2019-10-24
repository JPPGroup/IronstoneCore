using System;

namespace Jpp.Ironstone.Core.Helpers
{
    public static class AngleHelper
    {
        public static double ForSide(double angle, Side side)
        {
            return side switch
            {
                Side.Right => ForRightSide(angle),
                Side.Left => ForLeftSide(angle),
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, @"Invalid side"),
            };
        }

        public static double ForRightSide(double angle)
        {
            var output = angle - Constants.ANGLE_RADIANS_90_DEGREES;
            while (output < 0)
            {
                output += Constants.ANGLE_RADIANS_360_DEGREES;
            }
            return output;
        }

        public static double ForLeftSide(double angle)
        {
            var output = angle + Constants.ANGLE_RADIANS_90_DEGREES;
            while (output >= Constants.ANGLE_RADIANS_360_DEGREES)
            {
                output -= Constants.ANGLE_RADIANS_360_DEGREES;
            }
            return output;
        }
    }
}
