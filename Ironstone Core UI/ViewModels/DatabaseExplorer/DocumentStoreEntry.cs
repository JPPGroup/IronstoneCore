using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Jpp.Common;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core.UI.ViewModels.DatabaseExplorer
{
    class ManagerEntry : DatabaseEntry
    {
        public ICommand ResetCommand { get; set; }

        public IDrawingObjectManager Manager { get; set; }

        public ManagerEntry() : base()
        {

        }
    }
}
