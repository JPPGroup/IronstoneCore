using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jpp.AutoUpdate;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core
{
    class Objectmodel : IUpdateable
    {
        public Objectmodel(IModuleLoader modules)
        {
            /*var foundModules = modules.GetModules();
            
            foreach (Module foundModule in foundModules)
            {
                
            }

            AutoUpdate.Updater<Objectmodel>.Start(Constants.OBJECTMODEL_URL, );*/
        }

        public void Update()
        {

        }

        public string CompanyAttribute { get; } = "JPP Consulting";
        public string AppTitle { get; } = "JPP Ironstone";
        public Version InstalledVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;
    }
}
