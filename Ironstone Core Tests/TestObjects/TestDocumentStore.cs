using System;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core.Tests.TestObjects
{
    [Layer(Name = "TestDocumentStoreLayer")]
    public class TestDocumentStore : DocumentStore
    {
        public bool SaveTestProperty { get; set; }
        public TestDocumentStore(Document doc, Type[] managerTypes, ILogger log, LayerManager lm) : base(doc, managerTypes, log, lm) { }
    }
}
