using System;
using System.Collections.Generic;
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
        private Dictionary<string, Dictionary<Type, DocumentStore>> _stores;

        private bool _storeTypesInvalidated;

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
        private List<Type> _storesList;
        private List<Type> _managersList;

        public DataService(ILogger logger)
        {
            _current = this;
            _logger = logger;
            _stores = new Dictionary<string, Dictionary<Type, DocumentStore>>();

            //Add the document hooks
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.DocumentCreated += DocumentManagerOnDocumentCreated;
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.DocumentToBeDestroyed += DocumentManagerOnDocumentToBeDestroyed;
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
            if (!_stores.ContainsKey(ID))
            {
                _logger.Entry("Store not found.\n", Severity.Warning);
                Document doc = GetDocumentByName(ID);
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
            document.CommandEnded += (o, args) =>
            {
                foreach (DocumentStore documentStore in _stores[document.Name].Values)
                {
                    //TODO: Check this works
                    if (args.GlobalCommandName.Contains("regen"))
                    {
                        documentStore.ReenerateManagers();
                    }
                    else
                    {
                        documentStore.UpdateManagers();
                    }
                }
            };
        }

        private object CreateDocumentStore(Type T, Document doc)
        {
            DocumentStore ds = (DocumentStore) Activator.CreateInstance(T, doc, GetManagerTypes());
            ds.LoadWrapper();
            return ds;
        }

        public void PopulateStoreTypes()
        {
            _storesList = new List<Type>();
            _managersList = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    _storesList.AddRange(assembly.GetTypes().Where(t => typeof(DocumentStore).IsAssignableFrom(t)));
                    _managersList.AddRange(assembly.GetTypes().Where(t => typeof(IDrawingObjectManager).IsAssignableFrom(t)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
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
