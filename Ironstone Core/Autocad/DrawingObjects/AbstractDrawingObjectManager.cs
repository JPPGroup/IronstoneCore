using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;

namespace Jpp.Ironstone.Core.Autocad
{
    public abstract class AbstractDrawingObjectManager : IDrawingObjectManager
    {
        protected Document HostDocument;

        public AbstractDrawingObjectManager(Document document)
        {
            HostDocument = document;
        }

        public abstract void UpdateDirty();

        public abstract void UpdateAll();

        public abstract void Clear();

        public abstract void AllDirty();

        public abstract void ActivateObjects();
    }
}
