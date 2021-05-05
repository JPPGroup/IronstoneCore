using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    //TODO: Is required?
    internal class ModuleLoader : IModuleLoader
    {
        private readonly Dictionary<string, Module> _loadedModules;
        private readonly IAuthentication _authentication;
        private readonly ILogger<CoreExtensionApplication> _logger;
        private readonly IConfiguration _config;

        public string BinPath { get; set; }
        public string DataPath { get; set; }

        public ModuleLoader(IAuthentication authentication, ILogger<CoreExtensionApplication> logger, IConfiguration config)
        {
            _authentication = authentication;
            _logger = logger;
            _config = config;
            
            BinPath = Assembly.GetExecutingAssembly().Location;
            BinPath = BinPath.Substring(0, BinPath.LastIndexOf('\\'));
            DataPath = _config["AppData"];

            _loadedModules = new Dictionary<string, Module>();

            _logger.LogDebug($"Loading modules from {BinPath} and {DataPath}.");
            /*if (_config.EnableModuleUpdate)
            {
                ProcessManifest();
            }*/
        }

        public void Scan()
        {
            _loadedModules.Clear();
            //Iterate over every dll found in bin folder
            foreach (string dll in Directory.GetFiles(BinPath, "*.dll"))
            {
                GetAssemblyInfo(dll);
            }
            if (Directory.Exists(DataPath))
            {
                foreach (string dll in Directory.GetFiles(DataPath, "*.dll"))
                {
                    GetAssemblyInfo(dll);
                }
            }
        }

        public void Load()
        {
            if (!CoreExtensionApplication.CoreConsole)
            {
                try
                {
                    if (File.Exists(DataPath + "\\IronstoneCoreUI.dll"))
                    {
                        ExtensionLoader.Load(DataPath + "\\IronstoneCoreUI.dll");
                    }
                    else
                    {
                        ExtensionLoader.Load(BinPath + "\\IronstoneCoreUI.dll");
                    }
                    _logger.LogDebug("Core UI library loaded.");
                }
                catch (System.Exception e)
                {
                    _logger.LogCritical(e, $"Unable to load Core UI library");
                    throw;
                }
            }

            foreach (Module m in _loadedModules.Values.Where(m => m.ObjectModel))
            {
                if (m.Authenticated)
                {
                    LoadAssembly(m.Path);
                }
            }

            //Check if authenticated, otherwise block the auto loading
            if (_authentication.Authenticated())
            {
                foreach (Module m in _loadedModules.Values.Where(m => !m.ObjectModel))
                {
                    if (m.Authenticated)
                    {
                        LoadAssembly(m.Path);
                    }
                }
            }
        }

        /*private void UpdateManifest(string moduleFile)
        {
            if (File.Exists(moduleFile))
            {
                try
                {
                    File.Delete(moduleFile);
                }
                catch (System.Exception e)
                {
                    _logger.Entry($"Unable to delete existing module manifest file at {moduleFile}", Severity.Error);
                    _logger.LogException(e);
                }
            }

            using (var client = new WebClient())
            {
                string ModuleUrl = CoreExtensionApplication._current.Configuration.BaseUrl + CoreExtensionApplication._current.Configuration.ModuleManifest;
                try
                {
                    client.DownloadFile(ModuleUrl, moduleFile);
                }
                catch (System.Exception e)
                {
                    _logger.Entry($"Unable to download current module manifest file to {moduleFile} from {ModuleUrl}", Severity.Error);
                    _logger.LogException(e);
                }
            }
        }*/

        public IEnumerable<Module> GetModules()
        {
            return _loadedModules.Values;
        }

        private void LoadAssembly(string dll)
        {
            //Load the additional libraries found
            if (!ExtensionLoader.IsLoaded(dll))
            {
                Assembly target = ExtensionLoader.Load(dll);
                //TODO: Verify actually loaded

                _loadedModules[dll].Loaded = true;
            }
        }

        private void GetAssemblyInfo(string dll)
        {
            if (!dll.Contains("dpwin") && !dll.Contains("IronstoneCore"))
            {
                AssemblyName info = AssemblyName.GetAssemblyName(dll);
                Module m = new Module();
                m.Name = info.Name;
                m.Version = info.Version;
                m.UpdateAvailable = false;
                m.Loaded = false;
                m.Path = dll;
                if (dll.Contains("ObjectModel"))
                {
                    m.ObjectModel = true;
                }
                else
                {
                    m.ObjectModel = false;
                }

                if (m.Name.Contains("Ironstone"))
                {
                    if (_authentication.AuthenticateModule(dll))
                    {
                        m.Authenticated = true;
                    }
                    else
                    {
                        m.Authenticated = false;
                    }

                    //TODO: Verify actually loaded
                    if (ExtensionLoader.IsLoaded(dll))
                    {
                        m.Loaded = true;
                    }

                    _loadedModules.Add(dll, m);
                }
            }
        }
    
    }
}
