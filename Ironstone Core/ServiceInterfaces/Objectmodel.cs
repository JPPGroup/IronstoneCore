using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jpp.AutoUpdate;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    class Objectmodel : IUpdateable
    {
        public Objectmodel(IModuleLoader modules)
        {
            var module = modules.GetModules().First(m => m.Objectmodel);
            if(module != null)
                this.InstalledVersion = module.Version;

            AutoUpdate.Updater<Objectmodel>.Mandatory = true;
            AutoUpdate.Updater<Objectmodel>.UpdateMode = Mode.Forced;
            AutoUpdate.Updater<Objectmodel>.DownloadPath = modules.DataPath;
            AutoUpdate.Updater<Objectmodel>.ApplicationExitEvent += () => { modules.Load(); };
            AutoUpdate.Updater<Objectmodel>.Start(Constants.OBJECTMODEL_URL, this);
        }

        public void Update()
        {

        }

        public string CompanyAttribute { get; } = "JPP Consulting";
        public string AppTitle { get; } = "JPP Ironstone";
        public Version InstalledVersion { get; } = new Version(0,0,0,0);
    }
}
