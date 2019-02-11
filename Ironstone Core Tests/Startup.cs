using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests
{
    [SetUpFixture]
    class Startup
    {
        [OneTimeSetUp]
        public void Setup()
        {
            //ExtensionLoader.Load("IronstoneCore.dll");
        }
    }
}
