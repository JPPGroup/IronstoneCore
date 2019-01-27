using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Windows;
using AutoUpdaterDotNET;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Properties;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.ServiceInterfaces.Authentication;
using Jpp.Ironstone.Core.ServiceInterfaces.Loggers;
using Jpp.Ironstone.Core.UI;
using Jpp.Ironstone.Core.UI.Views;
using Unity;
using Unity.Lifetime;
using RibbonControl = Autodesk.Windows.RibbonControl;
using RibbonPanelSource = Autodesk.Windows.RibbonPanelSource;
using RibbonRowPanel = Autodesk.Windows.RibbonRowPanel;

[assembly: ExtensionApplication(typeof(CoreExtensionApplication))]
[assembly: CommandClass(typeof(CoreExtensionApplication))]

namespace Jpp.Ironstone.Core
{
    /// <summary>
    /// Loader class, the main entry point for the full application suite. Implements IExtensionApplication is it
    /// automatically initialised and terminated by AutoCad.
    /// </summary>
    class CoreExtensionApplication : IExtensionApplication
    {
        #region Public Variables

        public static CoreExtensionApplication _current;

        /// <summary>
        /// Returns true if currently running under the Core Console
        /// </summary>
        public static bool CoreConsole
        {
            get
            {
                if (_coreConsole != null) return _coreConsole.Value;
                try
                {
                    StatusBar unused = Application.StatusBar;
                    _coreConsole = false;
                }
                catch (System.Exception)
                {
                    _coreConsole = true;
                }

                return _coreConsole.Value;
            }
        }

        /// <summary>
        /// Returns true if currently running under Civil 3D
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static bool Civil3D
        {
            get
            {
                if (_civil3D != null) return _civil3D.Value;
                try
                {
                    //StatusBar testBar = Autodesk.AutoCAD.ApplicationServices.Application.StatusBar;
                    CivilDocument unused = CivilApplication.ActiveDocument;
                    _civil3D = true;
                }
                catch (System.Exception)
                {
                    _civil3D = false;
                }

                return _civil3D.Value;
            }
        }
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

        private UnityContainer _container;
        private ILogger _logger;
        private IAuthentication _authentication;
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
            if (CoreConsole)
                InitExtension();
            else
            {
                if (ComponentManager.Ribbon == null)
                {
                    Autodesk.AutoCAD.ApplicationServices.Core.Application.Idle += Application_Idle;
                }
                else
                {
                    //Ribbon existis, call the initialize method directly
                    InitExtension();
                }
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
            InitExtension();
        }

        #endregion

        #region Extension Setup

        /// <summary>
        /// Init JPP command loads all essential elements of the program, including the helper DLL files.
        /// </summary>
        public void InitExtension()
        {
            //Unity registration
            _container= new UnityContainer();
            //TODO: Add code here for choosing log type
            _container.RegisterInstance<ILogger>(new ConsoleLogger(), new ContainerControlledLifetimeManager());
            _container.RegisterInstance<IAuthentication>(new DinkeyAuthentication(), new ContainerControlledLifetimeManager());

            _logger = _container.Resolve<ILogger>();
            _logger.Entry(Resources.ExtensionApplication_Inform_LoadingMain);

            _container.RegisterInstance<IDataService>(new DataService(_logger), new ContainerControlledLifetimeManager());

            _authentication = _container.Resolve<IAuthentication>();

            if (!CoreConsole)
                CreateUi();
            
            //Load the additional DLL files, but only not if running in debug mode
#if !DEBUG
            Update();
#endif
            LoadModules();
            
            //Once all modules have been loaded inform data service
            _container.Resolve<IDataService>().PopulateStoreTypes();

            _logger.Entry(Resources.ExtensionApplication_Inform_LoadedMain);
        }

        public void CreateUi()
        {
            //Create the main UI
            RibbonTab ironstoneTab = CreateTab();
            CreateCoreMenu(ironstoneTab);
        }

        /// <summary>
        /// Creates the JPP tab and adds it to the ribbon
        /// </summary>
        /// <returns>The created tab</returns>
        public RibbonTab CreateTab()
        {
            RibbonControl rc = ComponentManager.Ribbon;
            RibbonTab ironstoneTab = new RibbonTab
            {
                Name = Constants.IRONSTONE_TAB_TITLE,
                Title = Constants.IRONSTONE_TAB_TITLE,
                Id = Constants.IRONSTONE_TAB_ID
            };

            rc.Tabs.Add(ironstoneTab);
            return ironstoneTab;
        }

        /// <summary>
        /// Add the core elements of the ui
        /// </summary>
        /// <param name="ironstoneTab">The tab to add the ui elements to</param>
        public void CreateCoreMenu(RibbonTab ironstoneTab)
        {
            RibbonPanel panel = new RibbonPanel();
            RibbonPanelSource source = new RibbonPanelSource { Title = "General" };
            RibbonRowPanel stack = new RibbonRowPanel();
            
            //stack.Items.Add(_settingsButton);
            RibbonToggleButton aboutButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_AboutWindow_Name, Resources.About,
                RibbonItemSize.Standard, Orientation.Horizontal, new About(), "10992236-c8f6-4732-b5e0-2d9194f07068");
            
            stack.Items.Add(new RibbonRowBreak());
            stack.Items.Add(aboutButton);

            //Add the new tab section to the main tab
            source.Items.Add(stack);
            panel.Source = source;
            ironstoneTab.Panels.Add(panel);
        }
        #endregion

        #region Updater
        /// <summary>
        /// Find all assemblies in the subdirectory, and load them into memory
        /// </summary>
        public void LoadModules()
        {
#if DEBUG
            string path = Assembly.GetExecutingAssembly().Location;
#else
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JPP Consulting\\JPP AutoCad Library";
#endif

            //Check if authenticated, otherwise block the auto loading
            if (_authentication.Authenticated())
            {
                //Iterate over every dll found in bin folder
                if (Directory.Exists(path))
                {
                    foreach (string dll in Directory.GetFiles(path, "*.dll"))
                    {
                        //Load the additional libraries found
                        Assembly target = ExtensionLoader.Load(dll);

                        //TODO: Pass _container to do injection here
                    }
                }
                else
                {
                    //Log.Entry(Resources.Error_ModuleDirectoryMissing, Severity.Error);
                }
            }
            else
            {
                //Log.Entry(Resources.Error_ModuleLoadFailedAuthentication, Severity.Error);
            }
        }
        
        // ReSharper disable once UnusedMember.Global
        public static void Update()
        {
            AutoUpdater.Start(Constants.INSTALLER_URL);
        }
        #endregion

        public static void RegisterExtension(IIronstoneExtensionApplication extension)
        {
            extension.InjectContainer(_current._container);
            extension.CreateUI();
        }
    }
}
