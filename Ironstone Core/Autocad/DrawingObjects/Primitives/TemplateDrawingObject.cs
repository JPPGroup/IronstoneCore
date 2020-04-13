using System;
using Jpp.Ironstone.Core.ServiceInterfaces.Template;

namespace Jpp.Ironstone.Core.Autocad
{
    // TODO: Add unit tests
    public class TemplateDrawingObject : BlockDrawingObject, ITemplatedObject
    {
        public Guid TemplateID { get; set; }
        public void UpdateFromTemplate(ITemplateSource source)
        {
            throw new NotImplementedException();
        }
    }
}
