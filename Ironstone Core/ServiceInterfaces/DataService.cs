using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public class DataService : IDataService
    {
        /// <summary>
        /// Stores that are currently loaded into memory
        /// </summary>
        private Dictionary<string, Dictionary<Type, DocumentStore>> _stores;

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
            if (_stores.ContainsKey(ID))
            {
                return (T) _stores[ID][typeof(T)];
            }
            else
            {
                _logger.Entry("Store not found.\n", Severity.Error);
                throw new ArgumentException();
            }
        }

        private void DocumentManagerOnDocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            if (_stores.ContainsKey(e.Document.Name))
            {
                _logger.Entry("Document already exists.\n", Severity.Error);
            }
            else
            {
                Dictionary<Type, DocumentStore> storeContainer = new Dictionary<Type, DocumentStore>();
                foreach (Type t in _storesList)
                {
                    storeContainer.Add(t, (DocumentStore) CreateDocumentStore(t, e.Document));
                }
                _stores.Add(e.Document.Name, storeContainer);

                e.Document.Database.BeginSave += (o, args) => SaveStores(e.Document.Name);
                e.Document.CommandEnded += (o, args) =>
                {
                    foreach (DocumentStore documentStore in _stores[e.Document.Name].Values)
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
    }
}
