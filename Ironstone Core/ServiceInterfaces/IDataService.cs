using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public interface IDataService
    {
        void CreateStoresFromAppDocumentManager();
        void PopulateStoreTypes();

        Type[] GetManagerTypes();

        T GetStore<T>(string ID) where T : DocumentStore;
    }
}
