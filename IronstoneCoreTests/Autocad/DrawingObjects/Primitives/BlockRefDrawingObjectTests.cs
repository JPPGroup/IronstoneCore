using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Tests.TestObjects;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.Autocad.DrawingObjects.Primitives
{
    [TestFixture]
    class BlockRefDrawingObjectTests : IronstoneTestFixture
    {
        public BlockRefDrawingObjectTests() : base(Assembly.GetExecutingAssembly(), typeof(BlockRefDrawingObjectTests),
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Test Drawings\\CivilTemplate.dwg")
        { }

        [Test]
        public void VerifyKnownDefaultProperties()
        {
            PropertiesInput pi = new PropertiesInput()
            {
                LayoutName = "CIV_A0L",
                BlockName = "A0 Mask"
            };

            PropertiesResponse respone = RunTest<PropertiesResponse>(nameof(VerifyKnownPropertiesResident), pi);
            Assert.AreEqual("", respone.Client);
            Assert.AreEqual("", respone.Project);
        }

        [Test]
        public void VerifyKnownProperties()
        {
            PropertiesInput pi = new PropertiesInput()
            {
                LayoutName = "CIV_A1L",
                BlockName = "A1 Mask"
            };

            PropertiesResponse respone = RunTest<PropertiesResponse>(nameof(VerifyKnownPropertiesResident), pi);
            Assert.AreEqual("Test Client", respone.Client);
            Assert.AreEqual("Test Project", respone.Project);
        }

        public PropertiesResponse VerifyKnownPropertiesResident(PropertiesInput pi)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                IEnumerable<BlockRefDrawingObject> records = doc.Database.GetLayout(pi.LayoutName).GetBlockReferences().Select(br => new TestBlockRefDrawingObject(doc, br));
                records = records.Where(br => br.BlockName == pi.BlockName);

                if (records.Count() != 1)
                {
                    throw new ArgumentOutOfRangeException();
                }

                BlockRefDrawingObject testObject = records.ElementAt(0);

                PropertiesResponse pr = new PropertiesResponse
                {
                    Client = testObject.GetProperty<string>("CLIENT1"),
                    Project = testObject.GetProperty<string>("PROJECT1")
                };

                return pr;
            }
        }

        [Serializable]
        public struct PropertiesInput
        {
            public string LayoutName { get; set; }
            public string BlockName { get; set; }
        }

        [Serializable]
        public struct PropertiesResponse
        {
            public string Client { get; set; }
            public string Project { get; set; }
        }
    }
}
