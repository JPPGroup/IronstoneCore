using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    internal class ModuleLoader : IModuleLoader
    {
        private Dictionary<string, Module> LoadedModules;

        private IAuthentication _authentication;
        private string binPath, dataPath;

        public ModuleLoader(IAuthentication authentication)
        {
            _authentication = authentication;

            binPath = Assembly.GetExecutingAssembly().Location;
            binPath = binPath.Substring(0, binPath.LastIndexOf('\\'));
            dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JPP Consulting\\Ironstone";

            LoadedModules = new Dictionary<string, Module>();
        }

        public void Scan()
        {
            //Iterate over every dll found in bin folder
            foreach (string dll in Directory.GetFiles(binPath, "*.dll"))
            {
                GetAssemblyInfo(dll);
            }
            if (Directory.Exists(dataPath))
            {
                foreach (string dll in Directory.GetFiles(dataPath, "*.dll"))
                {
                    GetAssemblyInfo(dll);
                }
            }
        }

        public void Load()
        {
            if (!CoreExtensionApplication.CoreConsole)
            {
                //LoadAssembly(binPath + "\\IronstoneCoreUI.dll");
                ExtensionLoader.Load(binPath + "\\IronstoneCoreUI.dll");
            }

            //Check if authenticated, otherwise block the auto loading
            if (_authentication.Authenticated())
            {
                foreach (Module m in LoadedModules.Values)
                {
                    if (m.Authenticated)
                    {
                        LoadAssembly(m.Path);
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
                m.Version = info.Version.ToString();
                m.UpdateAvailable = false;
                m.Loaded = false;
                m.Path = dll;

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
