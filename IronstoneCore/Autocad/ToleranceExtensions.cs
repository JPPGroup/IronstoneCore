using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class ToleranceExtensions
    {
        public static double GetAngle(this Tolerance tolerance)
        {
            return Math.PI / 360;
        }
    }
}
