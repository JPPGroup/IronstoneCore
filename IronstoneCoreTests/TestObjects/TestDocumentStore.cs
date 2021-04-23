using System;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Core.Tests.TestObjects
{
    [Layer(Name = "TestDocumentStoreLayer")]
    public class TestDocumentStore : DocumentStore
    {
        public bool SaveTestProperty { get; set; }
        public TestDocumentStore(Document doc, Type[] managerTypes, ILogger<CoreExtensionApplication> log, LayerManager lm, IConfiguration settings) : base(doc, managerTypes, log, lm, settings) { }
    }
}
