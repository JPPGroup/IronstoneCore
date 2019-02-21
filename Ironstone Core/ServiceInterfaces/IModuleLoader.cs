using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Common;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public interface IModuleLoader
    {
        void Scan();
        void Load();

        IEnumerable<Module> GetModules();
    }

    public class Module : BaseNotify
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        public bool UpdateAvailable { get; set; }
        public bool Objectmodel { get; set; }

        public bool Authenticated
        {
            get { return _authenticated; }
            set { SetField(ref _authenticated, value, "Authenticated"); }
        }

        private bool _authenticated;

        public bool Loaded
        {
            get { return _loaded; }
            set { SetField(ref _loaded, value, "Loaded"); }
        }

        private bool _loaded;
    }
}
