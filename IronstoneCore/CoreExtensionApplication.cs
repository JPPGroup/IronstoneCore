using System;
using System.Collections.Generic;
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
using Jpp.Ironstone.Core.Mocking;
using Jpp.Ironstone.Core.Properties;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.ServiceInterfaces.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

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
        private ILogger<IAuthentication> _authLogger;
        private IAuthentication _authentication;
        private List<IIronstoneExtensionApplication> _extensions;
        private bool _uiCreated;

        private IServiceCollection serviceCollection;
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
            _logger.LogInformation("Application Shutdown");
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
                _logger.LogTrace("Creating extensions UI");
                foreach (IIronstoneExtensionApplication ironstoneExtensionApplication in _extensions)
                {
                    ironstoneExtensionApplication.CreateUI();
                }
                _logger.LogTrace("UI Created");
            }

            _uiCreated = true;
            _logger.LogInformation("Application Startup Complete");
        }

        #endregion

        #region Extension Setup

        /// <summary>
        /// Init JPP command loads all essential elements of the program, including the helper DLL files.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public void InitExtension()
        {
            _extensions = new List<IIronstoneExtensionApplication>();
            _uiCreated = false;
            
            SetBindingRedirect();
            serviceCollection = BuildServiceCollection();

            try
            {
                _logger.LogInformation("===================");
                _logger.LogInformation("Application Startup");

                SetCustomAssemblyResolve();
                IConfiguration config = LoadConfiguration(_logger);
                serviceCollection.AddSingleton<IConfiguration>(config);
                
                _logger.LogInformation(Resources.ExtensionApplication_Inform_LoadingMain);
                _authentication = new PassDummyAuth(_authLogger);
                serviceCollection.AddSingleton<IAuthentication>(_authentication);
                serviceCollection.AddSingleton<ILogger<IAuthentication>>(_authLogger);
                
                IModuleLoader moduleLoader = new ModuleLoader(_authentication, _logger, config);
                serviceCollection.AddSingleton<IModuleLoader>(moduleLoader);
                
                _logger.LogTrace("Modules loading...");

                moduleLoader.Scan();
                moduleLoader.Load();

                _logger.LogTrace("Modules load complete");

                //TODO: Check position
                Container = serviceCollection.BuildServiceProvider();

                foreach (IIronstoneExtensionApplication ironstoneExtensionApplication in _extensions)
                {
                    ironstoneExtensionApplication.InjectContainer(Container);
                }
                
                IDataService dataService = Container.GetRequiredService<IDataService>();
                //TODO: This needed??
                DataService.Current.InvalidateStoreTypes();
                dataService.CreateStoresFromAppDocumentManager();
            }
            catch (System.Exception e)
            {
                _logger.LogCritical($"Exception thrown in core main resolver block - {e.Message}");
            }

            _logger.LogInformation(Resources.ExtensionApplication_Inform_LoadedMain);

            // If not running in civil 3d, hook into document creation events to monitor for civil3d drawings being opened
            if (!Civil3D)
            {
                _logger.LogInformation($"Application is not running in Civil3d, checking open documents...");
                foreach (Document doc in Application.DocumentManager)
                {
                    CheckDocument(doc);
                    AddRegAppKey(doc);
                }
                Application.DocumentManager.DocumentCreated += DocumentManagerOnDocumentCreated;
                _logger.LogInformation($"Document check finished");
            }
            else
            {
                _logger.LogInformation($"Application is running in Civil3d, document checks bypassed.");
            }

            _logger.LogTrace("Initi finished, awaiitng idle for UI construction");
        }

        private void SetCustomAssemblyResolve()
        {
            _logger.LogDebug("Wiring up custom assembly resolve");
            AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
            {
                string toassname = resolveArgs.Name.Split(',')[0];
                if (!CheckValidCustomResolve(toassname)) return null;

                _logger.LogDebug($"Fail assembly resolution for {resolveArgs.Name}.\nAttempting custom resolve.");
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
        }

        private bool CheckValidCustomResolve(string toassname)
        {
            if (!toassname.Contains("Ironstone")) return false;
            if (toassname.Contains("resources"))
            {
                _logger.LogWarning($"Localized resource for {CultureInfo.CurrentCulture} not found.");
                return false;
            }

            if (toassname.Contains("XmlSerializer"))
            {
                _logger.LogDebug($"Serialization library {toassname} not pregenerated");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Method overrides default binding as config file cannot be used
        /// </summary>
        private void SetBindingRedirect()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
            {
                if (resolveArgs.Name.Contains("resources"))
                {
                    return null;
                }

                //Bruteforce bidning redirect
                // Get just the name of assmebly
                // Aseembly name excluding version and other metadata
                string name = new Regex(",.*").Replace(resolveArgs.Name, string.Empty);

                if (name.Equals(resolveArgs.Name))
                    return null;
                
                // Load whatever version available
                return Assembly.Load(name);
            };
        }

        private IServiceCollection BuildServiceCollection()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            //serviceCollection.AddSingleton<IAuthentication, PassDummyAuth>();
            serviceCollection.AddSingleton<IDataService, DataService>();
            serviceCollection.AddSingleton<LayerManager>();
            serviceCollection.AddSingleton<IReviewManager, ReviewManager>();
            //serviceCollection.AddSingleton<IModuleLoader, ModuleLoader>();
            serviceCollection.AddSingleton<Civil3DAspect>();
            serviceCollection.AddSingleton<IronstoneCommandAspect>();
            serviceCollection.AddSingleton<ICivil3dController>(this);

            BuildLoggers(serviceCollection);
            return serviceCollection;
        }

        //TODO: Add in autocad console logger
        private void BuildLoggers(IServiceCollection collection)
        {
            string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\JPP Consulting\\Ironstone\\IronstoneLog.txt";

            var serilog = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.File(path, retainedFileCountLimit: 30, rollingInterval: RollingInterval.Day,
                    buffered: false, shared: true)
                .WriteTo.File("IronstoneLog.txt", retainedFileCountLimit: 30, rollingInterval: RollingInterval.Day,
                    buffered: false, shared: true)
                .CreateLogger();
            
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Default", LogLevel.Trace)
                    .AddConsole().AddSerilog(serilog, true);
            });

            collection.AddSingleton<ILoggerFactory>(loggerFactory);
            collection.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            _logger = loggerFactory.CreateLogger<CoreExtensionApplication>();
            _authLogger = loggerFactory.CreateLogger<IAuthentication>();
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
                _logger.LogWarning($"Civil3D features will not function in drawing {doc.Name}. Proceed at own risk.");
                Civil3DTagWarning?.Invoke(this, doc);
            }
            else
            {
                _logger.LogTrace($"{doc.Name} ok.");
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

            IConfiguration local = LoadAdditionalSettings(root, "config.json");
#if DEBUG
            _logger.LogDebug("User and network setting skipped as running in debug.");
            return local;
#endif
            IConfiguration network = LoadAdditionalSettingsFromValue(local, "Settings:NetworkPath");
            IConfiguration user = LoadAdditionalSettingsFromValue(network, "Settings:UserPath");
            return user;
        }

        private IConfiguration LoadAdditionalSettings(IConfiguration baseConfig, string path)
        {
            if (File.Exists(path))
            {
                ConfigurationBuilder builder = new ConfigurationBuilder();
                return builder.AddConfiguration(baseConfig).AddJsonFile(path).Build();
            }
            else
            {
                return baseConfig;
            }
        }

        private IConfiguration LoadAdditionalSettingsFromValue(IConfiguration baseConfig, string settings)
        {
            string networkPath = baseConfig[settings];
            if (!string.IsNullOrEmpty(networkPath))
            {
                if (File.Exists(networkPath))
                {
                    return LoadAdditionalSettings(baseConfig, networkPath);
                }
                else
                {
                    _logger.LogWarning($"Settings not found at {networkPath}");
                    return baseConfig;
                }
            }
            else
            {
                _logger.LogWarning($"Empty or missing settings value for {settings}");
                return baseConfig;
            }
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
            
            try
            {
                //extension.InjectContainer(_current.Container);
                extension.RegisterServices(serviceCollection);
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

