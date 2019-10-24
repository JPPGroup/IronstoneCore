using System;

namespace Jpp.Ironstone.Core
{
    public static class Constants
    {
        public const string IRONSTONE_CONCEPT_TAB_ID = "IRONSTONE_CONCEPT";
        public const string IRONSTONE_TAB_ID = "IRONSTONE_DESIGN";
        public const string REG_APP_NAME = "JPPI";

        public const double ANGLE_RADIANS_360_DEGREES = 2 * Math.PI;
        public const double ANGLE_RADIANS_270_DEGREES = 1.5 * Math.PI;
        public const double ANGLE_RADIANS_180_DEGREES = Math.PI;
        public const double ANGLE_RADIANS_90_DEGREES = Math.PI / 2;
        public const double ANGLE_RADIANS_0_DEGREES = 0;
        public const double ANGLE_TOLERANCE = (ANGLE_RADIANS_360_DEGREES / 360) * 0.2;
    }

    public enum Side { Left, Right }
}
