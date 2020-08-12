using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Jpp.AutoUpdate;
using Jpp.AutoUpdate.Classes;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Properties;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.ServiceInterfaces.Authentication;
using Jpp.Ironstone.Core.ServiceInterfaces.Loggers;
using Unity;
using Unity.Lifetime;

[assembly: ExtensionApplication(typeof(CoreExtensionApplication))]
[assembly: CommandClass(typeof(CoreExtensionApplication))]

namespace Jpp.Ironstone.Core
{
    /// <summary>
    /// Loader class, the main entry point for the full application suite. Implements IExtensionApplication is it
    /// automatically initialised and terminated by AutoCad.
    /// </summary>
    public class CoreExtensionApplication : IExtensionApplication, IUpdateable
    {
        #region Public Variables

        public static CoreExtensionApplication _current;

        [Obsolete("Configuration should be resolved via dependency injection")]
        public Configuration Configuration { get; set; }

        /// <summary>
        /// Returns true if currently running under the Core Console
        /// </summary>
        public static bool CoreConsole
        {
            get
            {
                if (_coreConsole != null) return _coreConsole.Value;
                /*try
                {
                    StatusBar unused = Application.StatusBar;
                    _coreConsole = false;
                }
                catch (System.Exception)
                {
                    _coreConsole = true;
                }*/
                if (System.Diagnostics.Process.GetCurrentProcess().ProcessName.Contains("accoreconsole"))
                {
                    _coreConsole = true;
                }
                else
                {
                    _coreConsole = false;
                }

                return _coreConsole.Value;
            }
        }

        /// <summary>
        /// Returns true if currently running under Civil 3D
        /// </summary>
        public static bool Civil3D
        {
            get
            {
                if (_civil3D != null) return _civil3D.Value;
                _civil3D = Civil3DTest();

                return _civil3D.Value;
            }
        }

        private static bool Civil3DTest()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.Contains("AeccDbMgd"));
        }

        /// <summary>
        /// Event fired when a civil 3d drawing is opened in other products. Default behaviour is to log a warning only.
        /// </summary>
        public event EventHandler<Document> Civil3DTagWarning;

        public UnityContainer Container { get; set; }

        #endregion

        #region Private variables
        /// <summary>
        /// Is software running under the core console? Null when this has not yet been checked
        /// </summary>
        private static bool? _coreConsole;

        /// <summary>
        /// Is software running under civil 3d? Null when this has not yet been checked
        /// </summary>
        private static bool? _civil3D;

        
        private ILogger _logger;
        private IAuthentication _authentication;
        private ObjectModel _objectModel;
        private List<IIronstoneExtensionApplication> _extensions;
        private bool _uiCreated;
        #endregion

        #region Autocad Extension Lifecycle
        /// <summary>
        /// Implement the Autocad extension api to load the additional libraries we need. Main library entry point
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void Initialize()
        {
            _current = this;

            //If not running in console only, detect if ribbon is currently loaded, and if not wait until the application is Idle.
            //Throws an error if try to add to the menu with the ribbon unloaded
            InitExtension();
            if (!CoreConsole)
            { 
                Autodesk.AutoCAD.ApplicationServices.Core.Application.Idle += Application_Idle;
            }
        }

        /// <summary>
        /// Implement the Autocad extension api to terminate the application
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void Terminate()
        {
        }

        /// <summary>
        /// Event handler to detect when the program is fully loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Idle(object sender, EventArgs e)
        {
            //Unhook the event handler to prevent multiple calls
            Autodesk.AutoCAD.ApplicationServices.Core.Application.Idle -= Application_Idle;
            //Call the initialize method now the application is loaded
            if (!CoreConsole)
            {
                foreach (IIronstoneExtensionApplication ironstoneExtensionApplication in _extensions)
                {
                    ironstoneExtensionApplication.CreateUI();
                }
            }

            _uiCreated = true;
        }

        #endregion

        #region Extension Setup

        /// <summary>
        /// Init JPP command loads all essential elements of the program, including the helper DLL files.
        /// </summary>
        public void InitExtension()
        {
            _extensions = new List<IIronstoneExtensionApplication>();
            _uiCreated = false;

            //Unity registration
            Container= new UnityContainer();

            //TODO: Add code here for choosing log type
            Container.RegisterType<ILogger, CollectionLogger>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAuthentication, DinkeyAuthentication>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IModuleLoader, ModuleLoader>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDataService, DataService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ObjectModel, ObjectModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IUserSettings, StandardUserSettings>(new ContainerControlledLifetimeManager());
            Container.RegisterType<LayerManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IReviewManager, ReviewManager>(new ContainerControlledLifetimeManager());
            Container.AddExtension(new Diagnostic());
            

            try
            {
                LoadConfiguration();
                _logger = Container.Resolve<ILogger>();
                _logger.LogEvent(Event.Message, "Application Startup");

                _logger.Entry("Wiring up custom assembly resolve");
                AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
                {
                    string toassname = resolveArgs.Name.Split(',')[0];
                    if (!toassname.Contains("Ironstone")) return null;
                    if (toassname.Contains("resources"))
                    {
                        _logger.Entry($"Localized resource for {CultureInfo.CurrentCulture} not found.", Severity.Information);
                        return null;
                    }
                    if (toassname.Contains("XmlSerializer"))
                    {
                        _logger.Entry($"Serialization library {toassname} not pregenerated", Severity.Debug);
                        return null;
                    }

                    _logger.Entry($"Fail assembly resolution for {resolveArgs.Name}.\nAttempting custom resolve.");
                    Assembly[] asmblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly ass in asmblies)
                    {
                        if (ass.FullName.Split(',')[0] == toassname)
                        {
                            return ass;
                        }
                    }

                    return null;
                };

                _logger.Entry(Resources.ExtensionApplication_Inform_LoadingMain);

                _authentication = Container.Resolve<IAuthentication>();

                IDataService dataService = Container.Resolve<IDataService>();

                Update();
            }
            catch (System.Exception e)
            {
                _logger.Entry($"Exception thrown in core main resolver block - {e.Message}", Severity.Crash);
            }

            _logger.Entry(Resources.ExtensionApplication_Inform_LoadedMain);

            // If not running in civil 3d, hook into document creation events to monitor for civil3d drawings being opened
            if (!Civil3D)
            {
                _logger.Entry($"Application is not running in Civil3d, checking documents...");
                foreach (Document doc in Application.DocumentManager)
                {
                    CheckDocument(doc);
                    AddRegAppKey(doc);
                }
                Application.DocumentManager.DocumentCreated += DocumentManagerOnDocumentCreated;
            }
            else
            {
                _logger.Entry($"Application is running in Civil3d, document checks bypassed.");
            }
        }

        private void DocumentManagerOnDocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            CheckDocument(e.Document);
            AddRegAppKey(e.Document);
        }

        private void CheckDocument(Document doc)
        {
            if (CheckDrawingForCivil3D(doc))
            {
                _logger.Entry("Civil3D features will not function in this drawing. Proceed at own risk.", Severity.Warning);
                Civil3DTagWarning?.Invoke(this, doc);
            }
        }

        private void AddRegAppKey(Document doc)
        {
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                RegAppTable regTable = (RegAppTable)trans.GetObject(doc.Database.RegAppTableId, OpenMode.ForRead);

                if(!regTable.Has("JPP"))
                {
                    regTable.UpgradeOpen();

                    // Add the application names that would be used to add Xdata
                    RegAppTableRecord app = new RegAppTableRecord();
                    app.Name = "JPP";

                    regTable.Add(app);
                    trans.AddNewlyCreatedDBObject(app, true);

                    trans.Commit();
                }
            }
        }

        private void LoadConfiguration()
        {
            XmlSerializer xml = new XmlSerializer(typeof(Configuration));
            string dll = Assembly.GetExecutingAssembly().Location;
            string containingFoler = dll.Remove(dll.LastIndexOf("\\"));
            string configPath = Path.Combine(containingFoler, "IronstoneConfig.xml");
            if (File.Exists(configPath))
            {
                using (Stream s = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Configuration = xml.Deserialize(s) as Configuration;
                }
            }
            else
            {
                Configuration = new Configuration();
            }

            //TODO: Remove once implemented iterator on serial dict
            for (int i = 0; i < Configuration.ContainerResolvers.Values.Count; i++)
            {
                Type from = Type.GetType(Configuration.ContainerResolvers.Keys.ElementAt(i));
                Type to = Type.GetType(Configuration.ContainerResolvers.Values.ElementAt(i));
                Container.RegisterType(from, to, new ContainerControlledLifetimeManager());
            }

            Container.RegisterInstance<Configuration>(Configuration, new ContainerControlledLifetimeManager());
        }

        #endregion

        #region Updater
        // ReSharper disable once UnusedMember.Global
        public void Update()
        {
            if (Configuration.EnableInstallerUpdate)
            {
                AutoUpdate.Updater<CoreExtensionApplication>.Start(CoreExtensionApplication._current.Configuration.InstallerUrl, this);
                AutoUpdate.Updater<CoreExtensionApplication>.CheckForUpdateEvent += (UpdateInfoEventArgs args) =>
                {
                    if (args == null)
                        return;
                    if (args.IsUpdateAvailable)
                    {
                        AutoUpdate.Updater<CoreExtensionApplication>.ShowUpdateForm();
                    }
                    else
                    {
                        _objectModel = Container.Resolve<ObjectModel>();
                    }
                };
                AutoUpdate.Updater<CoreExtensionApplication>.ApplicationExitEvent += () =>
                {
                    if (Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager
                            .MdiActiveDocument == null)
                    {
                        Autodesk.AutoCAD.ApplicationServices.Core.Application.Quit();
                        return;
                    }

                    Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager
                        .MdiActiveDocument?.SendStringToExecute("quit ", true, false, true);
                };
            }
            else
            {
                _objectModel = Container.Resolve<ObjectModel>();
                Container.Resolve<IModuleLoader>().Scan();
            }
        }
        #endregion

        public void RegisterExtension(IIronstoneExtensionApplication extension)
        {
            if (_uiCreated)
            {
                _logger.Entry("Extensions registration attempted after UI has been loaded", Severity.Error);
                return;
            }

            _logger.Entry($"{extension.GetType().ToString()} registration started", Severity.Debug);
            DataService.Current.InvalidateStoreTypes();
            try
            {
                extension.InjectContainer(_current.Container);
                _extensions.Add(extension);

                _logger.Entry($"{extension.GetType().ToString()} registration completed", Severity.Debug);
            }
            catch (System.Exception e)
            {
                _logger.Entry($"{extension.GetType().ToString()} registration failed", Severity.Error);
                _logger.LogException(e);
            }
        }

        public string CompanyAttribute { get; } = "JPP Consulting";
        public string AppTitle { get; } = "JPP Ironstone";
        public Version InstalledVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        #region Civil3D Flag

        public void MarkCurrentDrawingAsCivil3D()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                Transaction tr = doc.TransactionManager.TopTransaction;

                // Find the NOD in the database
                DBDictionary nod = (DBDictionary) tr.GetObject(doc.Database.NamedObjectsDictionaryId, OpenMode.ForWrite);

                // We use Xrecord class to store data in Dictionaries
                Xrecord plotXRecord = new Xrecord();
                ResultBuffer rb = new ResultBuffer();

                TypedValue tv = new TypedValue((int)DxfCode.Bool, true);
                rb.Add(tv);
                plotXRecord.Data = rb;

                // Create the entry in the Named Object Dictionary
                string id = this.GetType().FullName + "Civil3DRequired";
                nod.SetAt(id, plotXRecord);
                tr.AddNewlyCreatedDBObject(plotXRecord, true);
                trans.Commit();
            }
        }

        internal bool CheckDrawingForCivil3D(Document doc)
        {
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                // Find the NOD in the database
                DBDictionary nod = (DBDictionary)trans.GetObject(doc.Database.NamedObjectsDictionaryId, OpenMode.ForRead);
                string id = this.GetType().FullName + "Civil3DRequired";

                if (nod.Contains(id))
                {
                    ObjectId objId = nod.GetAt(id);
                    Xrecord XRecord = (Xrecord)trans.GetObject(objId, OpenMode.ForRead);
                    foreach (TypedValue value in XRecord.Data)
                    {
                        if (value.TypeCode == (short) DxfCode.Bool)
                        {
                            bool castValue = Convert.ToInt32(value.Value) != 0;
                            if (castValue)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        #endregion
    }
}
