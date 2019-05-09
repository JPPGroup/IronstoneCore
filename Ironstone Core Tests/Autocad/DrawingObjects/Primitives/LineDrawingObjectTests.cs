using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Jpp.Ironstone.Core.Tests.Autocad.DrawingObjects.Primitives.Tests
{
    [TestFixture()]
    class LineDrawingObjectTests : IronstoneTestFixture
    {
        public LineDrawingObjectTests() : base(Assembly.GetExecutingAssembly(), typeof(LineDrawingObjectTests)) { }

        [Test]
        public void VerifyIncorrectType()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyIncorrectType)));
        }

        public bool VerifyIncorrectTypeResident()
        {
            //Polyline pline = new Polyline();
            return false;
        }
    }
}
