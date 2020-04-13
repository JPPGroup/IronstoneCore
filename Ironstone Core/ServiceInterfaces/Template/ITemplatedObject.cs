using System;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Template
{
    public interface ITemplatedObject
    {
        Guid TemplateID { get; set; }

        void UpdateFromTemplate(ITemplateSource source);
    }
}
