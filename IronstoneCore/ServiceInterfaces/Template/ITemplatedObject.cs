using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Template
{
    public interface ITemplatedObject
    {
        Guid TemplateID { get; set; }

        void UpdateFromTemplate(ITemplateSource source);

        void TransferDrawingObject(Document destination, ObjectId newId);
    }
}
