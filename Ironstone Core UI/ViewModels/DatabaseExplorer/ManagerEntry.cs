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
    class DocumentStoreEntry : DatabaseEntry
    {
        public ICommand ResetCommand { get; set; }

        public DocumentStore Store { get; set; }

        public DocumentStoreEntry() : base()
        {
            ResetCommand = new DelegateCommand(() =>
            {
                Type t = Store.GetType();
                Store = DataService.Current.ResetStore(t, Host.Name);
            } );
        }
    }
}
