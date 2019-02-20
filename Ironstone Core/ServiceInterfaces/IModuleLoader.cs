using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public interface IModuleLoader
    {
        void Scan();
        void Load();

        IEnumerable<Module> GetModules();
    }

    public class Module
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        public bool UpdateAvailable { get; set; }
        public bool Authenticated { get; set; }
        public bool Loaded { get; set; }
    }
}
