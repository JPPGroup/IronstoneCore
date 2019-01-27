using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    interface IDataService
    {
        void PopulateStoreTypes();

        T GetStore<T>(string ID) where T : DocumentStore;
    }
}
