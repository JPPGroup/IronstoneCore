using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;

namespace Jpp.Ironstone.Core.Autocad
{
    public abstract class AbstractDrawingObjectManager : IDrawingObjectManager
    {
        [XmlIgnore]
        public Document HostDocument { get; set; }

        public AbstractDrawingObjectManager(Document document)
        {
            HostDocument = document;
        }

        public AbstractDrawingObjectManager()
        {
        }

        public abstract void UpdateDirty();

        public abstract void UpdateAll();

        public abstract void Clear();

        public abstract void AllDirty();

        public abstract void ActivateObjects();
    }
}
