using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Common;

namespace Jpp.Ironstone.Core
{
    public class Configuration
    {
        public SerializableDictionary<string, string> ContainerResolvers { get; set; }

        public bool EnableInstallerUpdate;
        public bool EnableObjectmodelUpdate;
        public bool EnableModuleUpdate;

        public Configuration()
        {
            ContainerResolvers = new SerializableDictionary<string, string>();
            EnableInstallerUpdate = true;
            EnableObjectmodelUpdate = true;
            EnableModuleUpdate = true;
        }
    }
}
