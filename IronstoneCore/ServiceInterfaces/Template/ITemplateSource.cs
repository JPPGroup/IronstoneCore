using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Template
{
    public interface ITemplateSource
    {
        IEnumerable<Guid> GetAllTemplates();

        (Database, TemplateDrawingObject) GetTemplate(Guid id);

        bool Contains(Guid id);
    }
}
