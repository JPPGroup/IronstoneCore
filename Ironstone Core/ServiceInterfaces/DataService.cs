using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public class DataService : IDataService
    {
        /// <summary>
        /// Stores that are currently loaded into memory
        /// </summary>
        internal Dictionary<string, Dictionary<Type, DocumentStore>> _stores;

        private bool _storeTypesInvalidated = true;

        public static DataService Current
        {
            get
            {
                if (_current == null)
                    throw new NullReferenceException();

                return _current;
            }
        }

        private static DataService _current;

        private ILogger _logger;
        internal List<Type> _storesList;
        internal List<Type> _managersList;

        public DataService(ILogger logger)
        {
            _current = this;
            _logger = logger;
            _stores = new Dictionary<string, Dictionary<Type, DocumentStore>>();

            //Add the document hooks
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.DocumentCreated += DocumentManagerOnDocumentCreated;
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.DocumentToBeDestroyed += DocumentManagerOnDocumentToBeDestroyed;

            //Incase the document has been loaded before the application (should only happen with manual loads...)
            foreach (Document d in Application.DocumentManager)
            {
                CreateStoresOnDocument(d);
                _logger.Entry("Document existed before extension was loaded.\n", Severity.Warning);
            }
        }

        private void DocumentManagerOnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            if (_stores.ContainsKey(e.Document.Name))
            {
                _stores.Remove(e.Document.Name);
            }
            else
            {
                _logger.Entry("Document does not exist.\n", Severity.Error);
            }
            
        }

        public T GetStore<T>(string ID) where T : DocumentStore
        {
            Document doc = GetDocumentByName(ID);

            if (!_stores.ContainsKey(ID))
            {
                _logger.Entry("Store not found.\n", Severity.Warning);
                if (doc != null)
                {
                    CreateStoresOnDocument(doc);
                }
                else
                {
                    _logger.Entry("Document not found for store creation.\n", Severity.Crash);
                    throw new ArgumentException();
                }
            }

            Dictionary<Type, DocumentStore> foundStores = _stores[ID];

            if (!foundStores.ContainsKey(typeof(T)))
            {
                foundStores.Add(typeof(T), (DocumentStore) CreateDocumentStore(typeof(T), doc));
            }

            return (T)_stores[ID][typeof(T)];
        }

        private void DocumentManagerOnDocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            if (_stores.ContainsKey(e.Document.Name))
            {
                _logger.Entry("Document already exists.\n", Severity.Error);
            }
            else
            {
                CreateStoresOnDocument(e.Document);
            }
        }

        private void CreateStoresOnDocument(Document document)
        {
            if(_storeTypesInvalidated)
                PopulateStoreTypes();

            Dictionary<Type, DocumentStore> storeContainer = new Dictionary<Type, DocumentStore>();
            foreach (Type t in _storesList)
            {
                storeContainer.Add(t, (DocumentStore) CreateDocumentStore(t, document));
            }

            _stores.Add(document.Name, storeContainer);

            document.Database.BeginSave += (o, args) => SaveStores(document.Name);
            document.CommandEnded += (o, args) => CommandEnded(document.Name, args.GlobalCommandName);
            document.CommandCancelled += (o, args) => CommandEnded(document.Name, args.GlobalCommandName);
        }

        private object CreateDocumentStore(Type T, Document doc)
        {
            if (_storeTypesInvalidated)
                PopulateStoreTypes();

            if (!_storesList.Contains(T))
            {
                _logger.Entry("Store type not recognised.\n", Severity.Crash);
                throw new ArgumentException();
            }

            DocumentStore ds = (DocumentStore) Activator.CreateInstance(T, doc, GetManagerTypes());
            ds.LoadWrapper();
            return ds;
        }

        public void PopulateStoreTypes()
        {
            _storesList = new List<Type>();
            _storesList.Add(typeof(DocumentStore));
            _managersList = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    _storesList.AddRange(assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(DocumentStore))));
                    _managersList.AddRange(assembly.GetTypes().Where(t => typeof(AbstractDrawingObjectManager).IsAssignableFrom(t)));
                }
                catch (Exception e)
                {
                    _logger.LogException(e);
                }
            }

            _storeTypesInvalidated = false;
        }

        public void InvalidateStoreTypes()
        {
            _storeTypesInvalidated = true;
        }

        private void SaveStores(string ID)
        {
            foreach (DocumentStore documentStore in _stores[ID].Values)
            {
                documentStore.SaveWrapper();
            }
        }

        private void CommandEnded(string DocName, string GlobalCommandName)
        {
            foreach (DocumentStore documentStore in _stores[DocName].Values)
            {
                //TODO: Check this works
                if (GlobalCommandName.Contains("regen"))
                {
                    documentStore.ReenerateManagers();
                }
                else
                {
                    documentStore.UpdateManagers();
                }
            }
        }

        public Type[] GetManagerTypes()
        {
            return _managersList.ToArray();
        }

        private Document GetDocumentByName(string Name)
        {
            foreach (Document d in Application.DocumentManager)
            {
                if (d.Name == Name)
                {
                    return d;
                }
            }

            return null;
        }

    }
}
