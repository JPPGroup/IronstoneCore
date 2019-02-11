using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Civil.ApplicationServices;
using AutoUpdaterDotNET;
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
    public class CoreExtensionApplication : IExtensionApplication
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
            _container= new UnityContainer();
            //TODO: Add code here for choosing log type
            _container.RegisterInstance<ILogger>(new ConsoleLogger(), new ContainerControlledLifetimeManager());
            _container.RegisterInstance<IAuthentication>(new DinkeyAuthentication(), new ContainerControlledLifetimeManager());

            _logger = _container.Resolve<ILogger>();
            _logger.Entry(Resources.ExtensionApplication_Inform_LoadingMain);

            _container.RegisterInstance<IDataService>(new DataService(_logger), new ContainerControlledLifetimeManager());

            _authentication = _container.Resolve<IAuthentication>();

            //Load the additional DLL files, but only not if running in debug mode
#if !DEBUG
            Update();
#endif
            LoadModules();
            
            //Once all modules have been loaded inform data service
            DataService.Current.PopulateStoreTypes();

            _logger.Entry(Resources.ExtensionApplication_Inform_LoadedMain);
        }
        #endregion

        #region Updater
        /// <summary>
        /// Find all assemblies in the subdirectory, and load them into memory
        /// </summary>
        public void LoadModules()
        {
            string binPath = Assembly.GetExecutingAssembly().Location;
            binPath = binPath.Substring(0, binPath.LastIndexOf('\\'));
            string dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JPP Consulting\\Ironstone";

            //Check if authenticated, otherwise block the auto loading
            if (_authentication.Authenticated())
            {
                //Iterate over every dll found in bin folder
                foreach (string dll in Directory.GetFiles(binPath, "*.dll"))
                {
                    //Load the additional libraries found
                    if (!ExtensionLoader.IsLoaded(dll))
                    {
                        //Skip protection dll, is this needed???
                        if (!dll.Contains("dpwin"))
                        {
                            Assembly target = ExtensionLoader.Load(dll);
                        }
                    }

                    //TODO: Pass _container to do injection here
                }
                if (Directory.Exists(dataPath))
                {
                    foreach (string dll in Directory.GetFiles(dataPath, "*.dll"))
                    {
                        //Load the additional libraries found
                        if (!ExtensionLoader.IsLoaded(dll))
                        {
                            Assembly target = ExtensionLoader.Load(dll);
                        }

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
            AutoUpdater.Start(Constants.INSTALLER_URL, Assembly.GetExecutingAssembly());
            AutoUpdater.ApplicationExitEvent += () => Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager
                .MdiActiveDocument.SendStringToExecute("quit ", true, false, true);
        }
        #endregion

        public static void RegisterExtension(IIronstoneExtensionApplication extension)
        {
            DataService.Current.InvalidateStoreTypes();
            extension.InjectContainer(_current._container);
            if (!CoreConsole)
            {
                extension.CreateUI();
            }
        }
    }
}
