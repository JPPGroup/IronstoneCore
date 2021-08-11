using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces.Library;
using Jpp.Ironstone.Core.ServiceInterfaces.Template;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class DataService : IDataService
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

        private ILogger<CoreExtensionApplication> _logger;
        private LayerManager _layerManager;
        private readonly IConfiguration _settings;
        internal List<Type> _storesList;
        internal List<Type> _managersList;

        public IReadOnlyList<LibraryNode> RootLibraries { get; private set; }

        public DataService(ILogger<CoreExtensionApplication> logger, LayerManager lm, IConfiguration settings)
        {
            _layerManager = lm;
            _settings = settings;
            _current = this;
            _logger = logger;
            _stores = new Dictionary<string, Dictionary<Type, DocumentStore>>();
            _templateSources = new List<ITemplateSource>();

            LoadLibraries();

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
                _logger.LogWarning("Document existed before extension was loaded");
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
                _logger.LogError("Document does not exist");
            }
        }

        public T GetStore<T>(string ID) where T : DocumentStore
        {
            Document doc = GetDocumentByName(ID);

            if (!_stores.ContainsKey(ID))
            {
                _logger.LogWarning("Store not found");
                if (doc != null)
                {
                    CreateStoresOnDocument(doc);
                }
                else
                {
                    _logger.LogCritical("Document not found for store creation");
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

        public IEnumerable<DocumentStore> GetExistingStores(string ID)
        {
            return _stores[ID].Values;
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
                _logger.LogError("Document already exists");
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

                if (!rat.Has(Constants.RegAppName))
                {
                    using (document.LockDocument())
                    {
                        rat.UpgradeOpen();
                        RegAppTableRecord ratr = new RegAppTableRecord
                        {
                            Name = Constants.RegAppName
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
                _logger.LogCritical("Store type not recognised");
                throw new ArgumentException("Store type not recognised.");
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
                        _logger.LogError(e, "Unknown error");
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
