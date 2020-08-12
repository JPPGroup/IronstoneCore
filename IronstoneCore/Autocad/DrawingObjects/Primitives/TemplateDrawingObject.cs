using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.ServiceInterfaces.Template;

namespace Jpp.Ironstone.Core.Autocad
{
    // TODO: Add unit tests
    public class TemplateDrawingObject : BlockDrawingObject, ITemplatedObject
    {
        public TemplateDrawingObject() : base()
        {
        }

        public TemplateDrawingObject(Database database) : base(database)
        {
        }
        
        public TemplateDrawingObject(Document document) : base(document)
        {
        }

        public Guid TemplateID { get; set; }
        public void UpdateFromTemplate(ITemplateSource source)
        {
            throw new NotImplementedException();
        }
    }
}
