using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    internal class ModuleLoader : IModuleLoader
    {
        private readonly Dictionary<string, Module> _loadedModules;
        private readonly IAuthentication _authentication;
        private readonly ILogger _logger;

        public string BinPath { get; set; }
        public string DataPath { get; set; }

        public ModuleLoader(IAuthentication authentication, ILogger logger)
        {
            _authentication = authentication;
            _logger = logger;
            
            BinPath = Assembly.GetExecutingAssembly().Location;
            BinPath = BinPath.Substring(0, BinPath.LastIndexOf('\\'));
            DataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JPP Consulting\\Ironstone";

            _loadedModules = new Dictionary<string, Module>();

            _logger.Entry($"Loading modules from {BinPath} and {DataPath}.", Severity.Debug);
            if (CoreExtensionApplication._current.Configuration.EnableModuleUpdate)
            {
                ProcessManifest();
            }
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
                    _logger.Entry("Core UI library loaded.", Severity.Debug);
                }
                catch (System.Exception e)
                {
                    _logger.Entry($"Unable to load Core UI library", Severity.Crash);
                    _logger.LogException(e);
                    throw;
                }
            }

            foreach (Module m in _loadedModules.Values.Where(m => m.Objectmodel))
            {
                if (m.Authenticated)
                {
                    LoadAssembly(m.Path);
                }
            }

            //Check if authenticated, otherwise block the auto loading
            if (_authentication.Authenticated())
            {
                foreach (Module m in _loadedModules.Values.Where(m => !m.Objectmodel))
                {
                    if (m.Authenticated)
                    {
                        LoadAssembly(m.Path);
                    }
                }
            }
        }

        public void ProcessManifest()
        {
            string moduleFile = DataPath + "\\ModuleManifest.txt";
            UpdateManifest(moduleFile);

            //Verify the file actually existis
            if (File.Exists(moduleFile))
            {
                try
                {
                    List<string> manifest = new List<string>();
                    using (TextReader reader = File.OpenText(moduleFile))
                    {
                        while (reader.Peek() != -1)
                        {
                            string m = reader.ReadLine();
                            manifest.Add(m);
                        }
                    }

                    string[] existingModules = Directory.GetFiles(DataPath);

                    foreach (string s in manifest)
                    {
                        bool found = false;
                        string fileName = s + ".dll";
                        string filePath = DataPath + "\\" + fileName;
                        foreach (string existingModule in existingModules)
                        {
                            if (existingModule == (filePath))
                            {
                                found = true;
                            }
                        }

                        if (!found)
                        {
                            if (_authentication.VerifyLicense(s))
                            {
                                using (var client = new WebClient())
                                {
                                    string downloadPath = Constants.BASE_URL + fileName;
                                    try
                                    {
                                        client.DownloadFile(downloadPath, filePath);
                                    }
                                    catch (System.Exception e)
                                    {
                                        _logger.Entry($"Unable to download module {s} from {downloadPath}", Severity.Error);
                                        _logger.LogException(e);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    _logger.Entry($"Unable to process latest manifest file {moduleFile}", Severity.Error);
                    _logger.LogException(e);
                }
            }
        }

        private void UpdateManifest(string moduleFile)
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
                string ModuleUrl = Constants.BASE_URL + "ModuleManifest.txt";
                try
                {
                    client.DownloadFile(ModuleUrl, moduleFile);
                }
                catch (System.Exception e)
                {
                    _logger.Entry($"Unable to donwload current module manifest file to {moduleFile} from {ModuleUrl}", Severity.Error);
                    _logger.LogException(e);
                }
            }
        }

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
                if (dll.Contains("Objectmodel"))
                {
                    m.Objectmodel = true;
                }
                else
                {
                    m.Objectmodel = false;
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
