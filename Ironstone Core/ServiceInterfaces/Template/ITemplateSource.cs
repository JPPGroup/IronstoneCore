using System;
using System.Collections.Generic;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Template
{
    public interface ITemplateSource
    {
        IEnumerable<Guid> GetAllTemplates();

        BlockDrawingObject GetTemplate(Guid id);

        bool Contains(Guid id);
    }
}
