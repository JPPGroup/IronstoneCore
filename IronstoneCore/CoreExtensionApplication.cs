using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Properties;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.ServiceInterfaces.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

[assembly: ExtensionApplication(typeof(CoreExtensionApplication))]
[assembly: CommandClass(typeof(CoreExtensionApplication))]

namespace Jpp.Ironstone.Core
{
    /// <summary>
    /// Loader class, the main entry point for the full application suite. Implements IExtensionApplication is it
    /// automatically initialised and terminated by AutoCad.
    /// </summary>
    public class CoreExtensionApplication : IExtensionApplication, ICivil3dController
    {
        #region Public Variables

        [Obsolete("Access should be resolved via dependency injection")]
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

        public IServiceProvider Container { get; private set; }

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

        
        private ILogger<CoreExtensionApplication> _logger;
        private IAuthentication _authentication;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public void InitExtension()
        {
            Debugger.Launch();
            _extensions = new List<IIronstoneExtensionApplication>();
            _uiCreated = false;

            IServiceCollection serviceCollection = BuildServiceCollection();

            try
            {
                
                _logger.LogInformation("Application Startup");

                SetCustomAssemblyResolve();
                serviceCollection.AddSingleton<IConfiguration>(LoadConfiguration(_logger));
                //TODO: Check position
                Container = serviceCollection.BuildServiceProvider();

                _logger.LogInformation(Resources.ExtensionApplication_Inform_LoadingMain);
                _authentication = Container.GetRequiredService<IAuthentication>();

                IDataService dataService = Container.GetRequiredService<IDataService>();
                
                Container.GetRequiredService<IModuleLoader>().Scan();
            }
            catch (System.Exception e)
            {
                _logger.LogCritical($"Exception thrown in core main resolver block - {e.Message}");
            }

            _logger.LogInformation(Resources.ExtensionApplication_Inform_LoadedMain);

            // If not running in civil 3d, hook into document creation events to monitor for civil3d drawings being opened
            if (!Civil3D)
            {
                _logger.LogInformation($"Application is not running in Civil3d, checking documents...");
                foreach (Document doc in Application.DocumentManager)
                {
                    CheckDocument(doc);
                    AddRegAppKey(doc);
                }
                Application.DocumentManager.DocumentCreated += DocumentManagerOnDocumentCreated;
            }
            else
            {
                _logger.LogInformation($"Application is running in Civil3d, document checks bypassed.");
            }
        }

        private void SetCustomAssemblyResolve()
        {
            _logger.LogDebug("Wiring up custom assembly resolve");
            AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
            {
                string toassname = resolveArgs.Name.Split(',')[0];
                //if (!toassname.Contains("Ironstone")) return null;
                if (toassname.Contains("resources"))
                {
                    _logger.LogWarning($"Localized resource for {CultureInfo.CurrentCulture} not found.");
                    return null;
                }

                if (toassname.Contains("XmlSerializer"))
                {
                    _logger.LogDebug($"Serialization library {toassname} not pregenerated");
                    return null;
                }

                _logger.LogDebug($"Fail assembly resolution for {resolveArgs.Name}.\nAttempting custom resolve.");
                Assembly[] asmblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly ass in asmblies)
                {
                    if (ass.FullName.Split(',')[0] == toassname)
                    {
                        return ass;
                    }
                }
                
                //Bruteforce bidning redirect
                // Get just the name of assmebly
                // Aseembly name excluding version and other metadata
                string name = new Regex(",.*").Replace(resolveArgs.Name, string.Empty);

                // Load whatever version available
                return Assembly.Load(name);
            };
        }

        private IServiceCollection BuildServiceCollection()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IAuthentication, DinkeyAuthentication>();
            serviceCollection.AddSingleton<IDataService, DataService>();
            serviceCollection.AddSingleton<LayerManager>();
            serviceCollection.AddSingleton<IReviewManager, ReviewManager>();

            var serilog = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File("IronstoneLog.txt", retainedFileCountLimit: 30, rollingInterval: RollingInterval.Day,
                    buffered: true)
                .CreateLogger();

            /*ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole()
                    .AddSerilog(serilog, true);
            });*/
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
            serviceCollection.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            _logger = loggerFactory.CreateLogger<CoreExtensionApplication>();

            //TODO: Add in file logger and autocad console logger

            return serviceCollection;
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
                _logger.LogWarning("Civil3D features will not function in this drawing. Proceed at own risk.");
                Civil3DTagWarning?.Invoke(this, doc);
            }
        }

        private static void AddRegAppKey(Document doc)
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

        private IConfiguration LoadConfiguration(ILogger<CoreExtensionApplication> logger)
        {
            ConfigurationBuilder rootBuilder = new ConfigurationBuilder();
            var resStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Jpp.Ironstone.Core.Resources.BaseConfig.json");
            IConfiguration root = rootBuilder.AddJsonStream(resStream).Build();


            IConfiguration local;
            if (File.Exists("config.json"))
            {
                ConfigurationBuilder localBuilder = new ConfigurationBuilder();
                local = rootBuilder.AddConfiguration(root).AddJsonFile("config.json").Build();
            }
            else
            {
                local = root;
            }

#if DEBUG
            _logger.LogDebug("User and network setting skipped as running in debug.");
            return local;
#endif
            string networkPath = local["Settings:NetworkPath"];
            IConfiguration network;
            if (!string.IsNullOrEmpty(networkPath) && File.Exists(networkPath))
            {
                ConfigurationBuilder localBuilder = new ConfigurationBuilder();
                network = rootBuilder.AddConfiguration(local).AddJsonFile(networkPath).Build();
            }
            else
            {
                _logger.LogWarning($"Network settings not found at {networkPath}");
                network = local;
            }

            string userPath = local["Settings:UserPath"];
            IConfiguration user;
            if (!string.IsNullOrEmpty(networkPath) && File.Exists(networkPath))
            {
                ConfigurationBuilder localBuilder = new ConfigurationBuilder();
                user = rootBuilder.AddConfiguration(network).AddJsonFile(userPath).Build();
            }
            else
            {
                _logger.LogWarning($"User settings not found at {userPath}");
                user = network;
            }

            return user;

            /*XmlSerializer xml = new XmlSerializer(typeof(Configuration));
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
                //Container.(from, to, new ContainerControlledLifetimeManager());
            }

            //Container.RegisterInstance<Configuration>(Configuration, new ContainerControlledLifetimeManager());*/
        }

#endregion

        public void RegisterExtension(IIronstoneExtensionApplication extension)
        {
            if (_uiCreated)
            {
                _logger.LogCritical("Extensions registration attempted after UI has been loaded");
                return;
            }

            _logger.LogDebug($"{extension.GetType().ToString()} registration started");
            DataService.Current.InvalidateStoreTypes();
            try
            {
                extension.InjectContainer(_current.Container);
                _extensions.Add(extension);

                _logger.LogInformation($"{extension.GetType().ToString()} registration completed");
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, $"{extension.GetType().ToString()} registration failed");
            }
        }

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
