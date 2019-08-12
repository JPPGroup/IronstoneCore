using System;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public interface ITemplatedObject
    {
        Guid TemplateID { get; set; }

        void UpdateFromTemplate(ITemplateSource source);
    }
}
