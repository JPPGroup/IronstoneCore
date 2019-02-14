using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    class ModuleLoader : IModuleLoader
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

        public void Load()
        {
            if (!CoreExtensionApplication.CoreConsole)
            {
                LoadAssembly(binPath + "\\IronstoneCoreUI.dll");
            }

            //Check if authenticated, otherwise block the auto loading
            if (_authentication.Authenticated())
            {
                //Iterate over every dll found in bin folder
                foreach (string dll in Directory.GetFiles(binPath, "*.dll"))
                {
                    LoadAssembly(dll);
                }
                if (Directory.Exists(dataPath))
                {
                    foreach (string dll in Directory.GetFiles(dataPath, "*.dll"))
                    {
                        LoadAssembly(dll);
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

        public IEnumerable<Module> GetModules()
        {
            return LoadedModules.Values;
        }

        private void LoadAssembly(string dll)
        {
            if (dll.Contains("Ironstone"))
            {
                //Load the additional libraries found
                if (!ExtensionLoader.IsLoaded(dll))
                {
                    //Skip protection dll, is this needed???
                    if (!dll.Contains("dpwin"))
                    {
                        AssemblyName info = AssemblyName.GetAssemblyName(dll);
                        Module m = new Module();
                        m.Name = info.Name;
                        m.Version = info.Version.ToString();
                        m.UpdateAvailable = false;

                        if (_authentication.AuthenticateModule(dll))
                        {
                            Assembly target = ExtensionLoader.Load(dll);
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

            //TODO: Pass _container to do injection here

        }
    }
}
