using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.Tests.TestObjects
{
    public class TestDocumentStore : DocumentStore
    {
        public TestDocumentStore(Document doc, Type[] ManagerTypes) : base(doc, ManagerTypes)
        {
        }
    }
}
