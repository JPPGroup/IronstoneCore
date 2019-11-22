using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
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

        internal List<ITemplateSource> _templateSources;

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
        private readonly IUserSettings _settings;
        internal List<Type> _storesList;
        internal List<Type> _managersList;

        public DataService(ILogger logger, LayerManager lm, IUserSettings settings)
        {
            _layerManager = lm;
            _settings = settings;
            _current = this;
            _logger = logger;
            _stores = new Dictionary<string, Dictionary<Type, DocumentStore>>();
            _templateSources = new List<ITemplateSource>();

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

        public ITemplateSource GetTemplateSource(Guid id)
        {
            foreach (ITemplateSource templateSource in _templateSources)
            {
                if (templateSource.Contains(id))
                {
                    return templateSource;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(id), "Template Id not found in available sources");
        }

        public void RegisterSource(ITemplateSource source)
        {
            _templateSources.Add(source);
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

        public void RegisterAppKey(Document document)
        {
            using (Transaction tr = document.TransactionManager.StartTransaction())
            {

                RegAppTable rat = (RegAppTable)tr.GetObject(document.Database.RegAppTableId, OpenMode.ForRead, false);

                if (!rat.Has(Constants.REG_APP_NAME))
                {
                    using (document.LockDocument())
                    {
                        rat.UpgradeOpen();
                        RegAppTableRecord ratr = new RegAppTableRecord
                        {
                            Name = Constants.REG_APP_NAME
                        };

                        rat.Add(ratr);
                        tr.AddNewlyCreatedDBObject(ratr, true);

                        tr.Commit();
                    }
                }
            }
        }

        private void CreateStoresOnDocument(Document document)
        {
            RegisterAppKey(document);

            if (_storeTypesInvalidated)
                PopulateStoreTypes();

            Dictionary<Type, DocumentStore> storeContainer = new Dictionary<Type, DocumentStore>();
            foreach (Type t in _storesList)
            {
                storeContainer.Add(t, (DocumentStore) CreateDocumentStore(t, document));
            }

            _stores.Add(document.Name, storeContainer);
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

            DocumentStore ds = (DocumentStore) Activator.CreateInstance(T, doc, GetManagerTypes(), _logger, _layerManager, _settings);
            ds.DocumentNameChanged += Store_DocumentNameChanged;
            ds.LoadWrapper();
            return ds;
        }

        private void Store_DocumentNameChanged(object sender, DocumentNameChangedEventArgs e)
        {
            if (!_stores.ContainsKey(e.OldName)) return;

            Dictionary<Type, DocumentStore> storesDictionary = _stores[e.OldName];
            _stores.Add(e.NewName, storesDictionary);
            _stores.Remove(e.OldName);
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
