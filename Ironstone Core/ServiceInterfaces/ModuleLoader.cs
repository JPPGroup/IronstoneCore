using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    internal class ModuleLoader : IModuleLoader
    {
        private Dictionary<string, Module> LoadedModules;
        private IAuthentication _authentication;
        private ILogger _logger;

        public string BinPath { get; set; }
        public string DataPath { get; set; }

        public ModuleLoader(IAuthentication authentication, ILogger logger)
        {
            _authentication = authentication;
            _logger = logger;

            BinPath = Assembly.GetExecutingAssembly().Location;
            BinPath = BinPath.Substring(0, BinPath.LastIndexOf('\\'));
            DataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JPP Consulting\\Ironstone";

            LoadedModules = new Dictionary<string, Module>();

            ProcessManifest();
        }

        public void Scan()
        {
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
                //LoadAssembly(BinPath + "\\IronstoneCoreUI.dll");
                ExtensionLoader.Load(BinPath + "\\IronstoneCoreUI.dll");
            }

            foreach (Module m in LoadedModules.Values.Where(m => m.Objectmodel))
            {
                if (m.Authenticated)
                {
                    LoadAssembly(m.Path);
                }
            }

            //Check if authenticated, otherwise block the auto loading
            if (_authentication.Authenticated())
            {
                foreach (Module m in LoadedModules.Values.Where(m => !m.Objectmodel))
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
            if (File.Exists(BinPath + "\\ModuleManifest.txt"))
            {
                File.Delete(BinPath + "\\ModuleManifest.txt");
            }

            using (var client = new WebClient())
            {
                client.DownloadFile(Constants.BASE_URL + "ModuleManifest.txt", DataPath + "\\ModuleManifest.txt");
            }

            List<string> manifest = new List<string>();
            using (TextReader reader = File.OpenText(DataPath + "\\ModuleManifest.txt"))
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
                foreach (string existingModule in existingModules)
                {
                    if (existingModule == (DataPath + "\\" + fileName))
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
                            try
                            {
                                client.DownloadFile(Constants.BASE_URL + fileName, DataPath + "\\" + fileName);
                            }
                            catch (System.Exception e)
                            {
                                _logger.Entry($"Module {s} not found", Severity.Warning);
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<Module> GetModules()
        {
            return LoadedModules.Values;
        }

        private void LoadAssembly(string dll)
        {
            //Load the additional libraries found
            if (!ExtensionLoader.IsLoaded(dll))
            {
                Assembly target = ExtensionLoader.Load(dll);
                //TODO: Verify actually loaded

                LoadedModules[dll].Loaded = true;
            }

            //TODO: Pass _container to do injection here
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
                    LoadedModules.Add(dll, m);
                }
            }
        }
    
    }
}
