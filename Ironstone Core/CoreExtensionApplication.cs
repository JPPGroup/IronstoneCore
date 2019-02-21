using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Civil.ApplicationServices;
using Jpp.AutoUpdate;
using Jpp.Ironstone.Core;
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

        public Control SyncContext;

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
        private Objectmodel _objectmodel;
        #endregion

        #region Autocad Extension Lifecycle
        /// <summary>
        /// Implement the Autocad extension api to load the additional libraries we need. Main library entry point
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void Initialize()
        {
            SyncContext = new Control();
            SyncContext.CreateControl();

            _current = this;

            //If not running in console only, detect if ribbon is currently loaded, and if not wait until the application is Idle.
            //Throws an error if try to add to the menu with the ribbon unloaded
            if (CoreConsole)
                InitExtension();
            else
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
            Container= new UnityContainer();
            //TODO: Add code here for choosing log type
            Container.RegisterType<ILogger, CollectionLogger>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAuthentication, DinkeyAuthentication>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IModuleLoader, ModuleLoader>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDataService, DataService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<Objectmodel, Objectmodel>(new ContainerControlledLifetimeManager());

            _logger = Container.Resolve<ILogger>();
            _logger.Entry(Resources.ExtensionApplication_Inform_LoadingMain);
            _authentication = Container.Resolve<IAuthentication>();

            Container.Resolve<IModuleLoader>().Scan();

            //Load the additional DLL files, but only not if running in debug mode
#if !DEBUG
            Update();
#endif
            IDataService dataService = Container.Resolve<IDataService>();

            //Container.Resolve<IModuleLoader>().Load();

            //Once all modules have been loaded inform data service
            dataService.PopulateStoreTypes();

            _logger.Entry(Resources.ExtensionApplication_Inform_LoadedMain);
        }
        #endregion

        #region Updater
        // ReSharper disable once UnusedMember.Global
        public void Update()
        {
            AutoUpdate.Updater<CoreExtensionApplication>.Start(Constants.INSTALLER_URL, this);
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

            _objectmodel = Container.Resolve<Objectmodel>();
        }
        #endregion

        public void RegisterExtension(IIronstoneExtensionApplication extension)
        {
            DataService.Current.InvalidateStoreTypes();
            extension.InjectContainer(_current.Container);
            if (!CoreConsole)
            {
                extension.CreateUI();
            }
        }

        public string CompanyAttribute { get; } = "JPP Consulting";
        public string AppTitle { get; } = "JPP Ironstone";
        public Version InstalledVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;
    }
}
