using System;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core.Tests.TestObjects
{
    public class TestDocumentStore : DocumentStore
    {
        public TestDocumentStore(Document doc, Type[] managerTypes, ILogger log) : base(doc, managerTypes, log) { }
    }
}
