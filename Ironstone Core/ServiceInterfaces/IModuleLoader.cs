using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public interface IModuleLoader
    {
        void Load();

        IEnumerable<Module> GetModules();
    }

    public struct Module
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool UpdateAvailable { get; set; }
        public bool Authenticated { get; set; }
    }
}
