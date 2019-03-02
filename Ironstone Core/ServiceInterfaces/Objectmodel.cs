using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jpp.AutoUpdate;
using Jpp.AutoUpdate.Classes;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    class Objectmodel : IUpdateable
    {
        public Objectmodel(IModuleLoader modules)
        {
            var module = modules.GetModules().FirstOrDefault(m => m.Objectmodel);
            if(module != null)
                this.InstalledVersion = module.Version;

            AutoUpdate.Updater<Objectmodel>.Mandatory = true;
            AutoUpdate.Updater<Objectmodel>.UpdateMode = Mode.ForcedDownload;
            AutoUpdate.Updater<Objectmodel>.DownloadPath = modules.DataPath;
            AutoUpdate.Updater<Objectmodel>.CheckForUpdateEvent += (UpdateInfoEventArgs args) =>
            {
                if (args == null)
                    return;
                if (args.IsUpdateAvailable)
                {
                    AutoUpdate.Updater<Objectmodel>.DownloadUpdate();
                    AutoUpdate.Updater<Objectmodel>.Exit();
                    modules.Scan();
                    modules.Load();
                }
                else
                {
                    modules.Load();
                }
            };
            AutoUpdate.Updater<Objectmodel>.ApplicationExitEvent += () =>
            {
                int i = 0;
            };

            if (CoreExtensionApplication._current.Configuration.EnableObjectmodelUpdate)
            {
                AutoUpdate.Updater<Objectmodel>.Start(Constants.OBJECTMODEL_URL, this);
            }
            else
            {
                modules.Load();
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
