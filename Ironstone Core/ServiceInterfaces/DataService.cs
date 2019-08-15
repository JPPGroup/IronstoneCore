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
        private LayerManager _layerManager;
        internal List<Type> _storesList;
        internal List<Type> _managersList;

        public DataService(ILogger logger, LayerManager lm)
        {
            _layerManager = lm;
            _current = this;
            _logger = logger;
            _stores = new Dictionary<string, Dictionary<Type, DocumentStore>>();

            //Add the document hooks
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.DocumentCreated += DocumentManagerOnDocumentCreated;
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.DocumentToBeDestroyed += DocumentManagerOnDocumentToBeDestroyed;

            //Incase the document has been loaded before the application (should only happen with manual loads...)
            CreateStoresFromAppDocumentManager();
        }

        public void CreateStoresFromAppDocumentManager()
        {
            //Clear and force reload of stores
            _stores.Clear();
            _storeTypesInvalidated = true;

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
            return (T)getStore(typeof(T), ID, false);
        }

        private DocumentStore getStore(Type T, string ID, bool reset)
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

            if (foundStores.ContainsKey(T) && reset)
            {
                foundStores.Remove(T);
            }

            if (!foundStores.ContainsKey(T))
            {
                foundStores.Add(T, (DocumentStore) CreateDocumentStore(T, doc));
            }

            return _stores[ID][T];
        }

        public IEnumerable<DocumentStore> GetAllStores(string ID)
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
                    _logger.Entry("Document not found for get all stores.\n", Severity.Crash);
                    throw new ArgumentException();
                }
            }

            return _stores[ID].Values;
        }

        public T ResetStore<T>(string ID) where T : DocumentStore
        {
            return (T)getStore(typeof(T), ID, true);
        }

        public DocumentStore ResetStore(Type T, string ID)
        {
            if(!T.IsAssignableFrom(typeof(DocumentStore)))
                throw new ArgumentException($"Passed type {T} is not a document store.");

            return getStore(T, ID, true);
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

            DocumentStore ds = (DocumentStore) Activator.CreateInstance(T, doc, GetManagerTypes(), _logger, _layerManager);
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
                if (assembly.FullName.Contains("Ironstone"))
                {
                    try
                    {
                        _storesList.AddRange(assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(DocumentStore))));
                        _managersList.AddRange(assembly.GetTypes()
                            .Where(t => typeof(IDrawingObjectManager).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface));
                    }
                    catch (Exception e)
                    {
                        _logger.LogException(e);
                    }
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
                if (GlobalCommandName.ToLower().Contains("regen"))
                {
                    documentStore.RegenerateManagers();
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
