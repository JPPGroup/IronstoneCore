using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Mocking;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
#pragma warning disable CS0618

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

        public static bool ForgeDesignAutomation
        {
            get
            {
                return (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAS_WORKITEM_ID")) || !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("FORGE")));
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
            SetBindingRedirect();

            //If not running in console only, detect if ribbon is currently loaded, and if not wait until the application is Idle.
            //Throws an error if try to add to the menu with the ribbon unloaded
            try
            {
                InitExtension();
            }
            catch (System.Exception ex)
            {
                _logger.LogCritical(ex, "Core initialisation failed with unknown error.");
            }
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

            serviceCollection = BuildServiceCollection();

            try
            {
                _logger.LogInformation("===================");
                _logger.LogInformation("Application Startup");

                SetCustomAssemblyResolve();
                IConfiguration config = LoadConfiguration(_logger);
                serviceCollection.AddSingleton<IConfiguration>(config);

                _logger.LogInformation("Core extension loading begun...");
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
                _logger.LogCritical(e, "Exception thrown in core main resolver block");

            }

            _logger.LogInformation("Core loaded successfully.");

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

            _logger.LogTrace("Init finished, awaitng application idle for UI construction");
        }

        private void SetCustomAssemblyResolve()
        {
            _logger.LogDebug("Wiring up custom assembly resolve");
            AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
            {
                string toassname = resolveArgs.Name.Split(',')[0];
                if (!CheckValidCustomResolve(toassname)) return null;

                _logger.LogDebug("Fail assembly resolution for {Name}. Attempting custom resolve.", resolveArgs.Name);
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
                _logger.LogTrace("Localized resource for {CurrentCulture} not found.", CultureInfo.CurrentCulture);
                return false;
            }

            if (toassname.Contains("XmlSerializer"))
            {
                _logger.LogTrace("Serialization library {toassname} not pregenerated", toassname);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Method overrides default binding as config file cannot be used
        /// </summary>
        private static void SetBindingRedirect()
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

                if (name.Equals(resolveArgs.Name, StringComparison.Ordinal))
                    return null;

                // Load whatever version available
                return Assembly.Load(name);
            };
        }

        private IServiceCollection BuildServiceCollection()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IAuthentication, PassDummyAuth>();
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

        private void BuildLoggers(IServiceCollection collection)
        {
            string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\JPP Consulting\\Ironstone\\IronstoneLog.txt";
            string localPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "IronstoneLog.txt");

            var serilog = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .MinimumLevel.Verbose()
                .WriteTo.File(path, retainedFileCountLimit: 30, rollingInterval: RollingInterval.Day,
                    buffered: false, shared: true)
                .WriteTo.File(localPath, retainedFileCountLimit: 30, rollingInterval: RollingInterval.Day,
                    buffered: false, shared: true)
#if !DEBUG
                .WriteTo.Seq("http://seqingest.services.cedarbarn.local:5341", apiKey: "Cp5KJHS7eelC1u2z7Tn5")
#endif
                .CreateLogger();

            Microsoft.Extensions.Logging.LogLevel level;

            if (ForgeDesignAutomation)
            {
                level = Microsoft.Extensions.Logging.LogLevel.Trace;
            }
            else
            {
                level = Microsoft.Extensions.Logging.LogLevel.Warning;
            }

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter(null, LogLevel.Trace).AddSerilog(serilog, true);
                builder.AddFilter<AcConsoleLoggerProvider>(null, level).AddAcConsoleLogger();
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
                _logger.LogWarning("Civil3D features will not function in drawing {doc}. Proceed at own risk.", doc.Name);
                Civil3DTagWarning?.Invoke(this, doc);
            }
            else
            {
                _logger.LogTrace("{doc} ok.", doc.Name);
            }
        }

        private static void AddRegAppKey(Document doc)
        {
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                RegAppTable regTable = (RegAppTable)trans.GetObject(doc.Database.RegAppTableId, OpenMode.ForRead);

                if (!regTable.Has("JPP"))
                {
                    regTable.UpgradeOpen();

                    // Add the application names that would be used to add Xdata
                    RegAppTableRecord app = new RegAppTableRecord
                    {
                        Name = "JPP"
                    };

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

            string localPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.json");
            IConfiguration local = LoadAdditionalSettings(root, localPath);

#if DEBUG
            _logger.LogDebug("User and network setting skipped as running in debug.");
            return local;
#else
            IConfiguration network = LoadAdditionalSettingsFromValue(local, "Settings:NetworkPath");
            IConfiguration user = LoadAdditionalSettingsFromValue(network, "Settings:UserPath");

            string workingPath = "config.json";
            IConfiguration working = LoadAdditionalSettings(user, workingPath);

            return working;
#endif
        }

        private IConfiguration LoadAdditionalSettings(IConfiguration baseConfig, string path)
        {
            if (File.Exists(path))
            {
                _logger.LogInformation("Loading settings from {path}", path);
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
                    _logger.LogWarning("Settings not found at {networkPath}", networkPath);
                    return baseConfig;
                }
            }
            else
            {
                _logger.LogWarning("Empty or missing settings value for {settings}", settings);
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

            _logger.LogDebug("{extension} registration started", extension.GetType().ToString());

            try
            {
                //extension.InjectContainer(_current.Container);
                extension.RegisterServices(serviceCollection);
                _extensions.Add(extension);

                _logger.LogInformation("{extension} registration completed", extension.GetType().ToString());
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, "{extension} registration failed", extension.GetType().ToString());
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
                DBDictionary nod = (DBDictionary)tr.GetObject(doc.Database.NamedObjectsDictionaryId, OpenMode.ForWrite);

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
                        if (value.TypeCode == (short)DxfCode.Bool)
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

        [CommandMethod("IRONSTONE_HELLOWORLD")]
        [IronstoneCommand]
        public static void HelloWorld()
        {
            HelloWordStructure structure = new HelloWordStructure();
            string contents = JsonSerializer.Serialize(structure);

            string path;
            if (!ForgeDesignAutomation)
            {
                path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "helloworld.json");
            }
            else
            {
                path = "helloworld.json";
            }
            File.WriteAllText(path, contents);
        }
    }
}

