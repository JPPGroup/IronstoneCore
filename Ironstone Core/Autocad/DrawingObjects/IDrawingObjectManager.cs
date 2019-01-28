using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Core.Autocad
{
    public interface IDrawingObjectManager
    {
        void UpdateDirty();

        void UpdateAll();

        void Clear();

        void AllDirty();

        void ActivateObjects();
    }
}
