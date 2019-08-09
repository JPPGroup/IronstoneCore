using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jpp.AutoUpdate;
using Jpp.AutoUpdate.Classes;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    class ObjectModel : IUpdateable
    {
        public ObjectModel(IModuleLoader modules, IDataService dataService)
        {
            modules.Scan(); // Scan to ensure we have an up to date list of modules. 

            var module = modules.GetModules().FirstOrDefault(m => m.ObjectModel);
            if(module != null)
                this.InstalledVersion = module.Version;

            AutoUpdate.Updater<ObjectModel>.Mandatory = true;
            AutoUpdate.Updater<ObjectModel>.UpdateMode = Mode.ForcedDownload;
            AutoUpdate.Updater<ObjectModel>.DownloadPath = modules.DataPath;
            AutoUpdate.Updater<ObjectModel>.CheckForUpdateEvent += (UpdateInfoEventArgs args) =>
            {
                if (args == null)
                    return;
                if (args.IsUpdateAvailable)
                {
                    AutoUpdate.Updater<ObjectModel>.DownloadUpdate();
                    AutoUpdate.Updater<ObjectModel>.Exit();
                    modules.Scan();               
                }

                modules.Load(dataService);
            };
            AutoUpdate.Updater<ObjectModel>.ApplicationExitEvent += () =>
            {
                int i = 0;
            };

            if (CoreExtensionApplication._current.Configuration.EnableObjectModelUpdate)
            {
                AutoUpdate.Updater<ObjectModel>.Start(CoreExtensionApplication._current.Configuration.ObjectModelUrl, this);
            }
            else
            {
                modules.Scan();
                modules.Load(dataService);                
            }
        }

        public void Update()
        {

        }

        public string CompanyAttribute { get; } = "JPP Consulting";
        public string AppTitle { get; } = "JPP Ironstone Datamodel";
        public Version InstalledVersion { get; } = new Version(0,0,0,0);
    }
}
