using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.UI.ViewModels.DatabaseExplorer;

namespace Jpp.Ironstone.Core.UI.ViewModels
{
    public class DatabaseExplorerViewModel
    {
        private Document _targetDocument;

        public ObservableCollection<DatabaseEntry> DatabaseRoots { get; set; }

        public DatabaseExplorerViewModel(Document target)
        {
            _targetDocument = target;

            DatabaseRoots = new ObservableCollection<DatabaseEntry>();
            ParseDatabase();
        }

        private void ParseDatabase()
        {
            DatabaseRoots.Clear();

            DatabaseEntry docStores = new DatabaseEntry()
            {
                Name = "Document Stores",
                Host = _targetDocument
            };
            IEnumerable<DocumentStore> stores = DataService.Current.GetAllStores(_targetDocument.Name);

            foreach (DocumentStore documentStore in stores)
            {
                DocumentStoreEntry documentStoreEntry = new DocumentStoreEntry()
                {
                    Name = documentStore.GetType().Name,
                    Store = documentStore,
                    Host = _targetDocument
                };

                docStores.Children.Add(documentStoreEntry);
            }

            DatabaseRoots.Add(docStores);

            ParseNod();
            //Dictionaries taken from https://knowledge.autodesk.com/search-result/caas/CloudHelp/cloudhelp/2017/ENU/OARXMAC-DevGuide/files/GUID-83ABF20A-57D4-4AB3-8A49-D91E0F70DBFF-htm.html
            ParseBlockTable();
            ParseLayerTable();
            ParseViewTable();
            ParseRegAppTable();
        }

        private void ParseNod()
        {
            Transaction tr = _targetDocument.Database.TransactionManager.TopTransaction;

            // Find the NOD in the database
            DBDictionary nod = (DBDictionary)tr.GetObject(_targetDocument.Database.NamedObjectsDictionaryId, OpenMode.ForRead);

            DatabaseEntry nodEntries = new DatabaseEntry()
            {
                Host = _targetDocument,
                Name = "Named Object Dictionaries"
            };

            foreach (DBDictionaryEntry dbDictionaryEntry in nod)
            {
                DatabaseEntry de = new DatabaseEntry()
                {
                    Host = _targetDocument,
                    Name = dbDictionaryEntry.Key
                };

                nodEntries.Children.Add(de);
            }

            DatabaseRoots.Add(nodEntries);
        }

        private void ParseRegAppTable()
        {
            
        }

        private void ParseViewTable()
        {
            
        }

        private void ParseLayerTable()
        {
            
        }

        private void ParseBlockTable()
        {
            
        }
    }
}
