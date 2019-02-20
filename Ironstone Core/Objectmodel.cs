using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoUpdate;
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
    }
}
