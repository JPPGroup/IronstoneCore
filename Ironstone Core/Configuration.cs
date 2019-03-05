using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Common;
using Jpp.Ironstone.Core.Mocking;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core
{
    public class Configuration
    {
        public SerializableDictionary<string, string> ContainerResolvers { get; set; }

        public bool EnableInstallerUpdate;
        public bool EnableObjectModelUpdate;
        public bool EnableModuleUpdate;

        public Configuration()
        {
            ContainerResolvers = new SerializableDictionary<string, string>();
            EnableInstallerUpdate = true;
            EnableObjectModelUpdate = true;
            EnableModuleUpdate = true;
        }

        public void TestSettings()
        {
            EnableInstallerUpdate = false;
            EnableModuleUpdate = false;
            EnableObjectModelUpdate = false;
            ContainerResolvers.Add(typeof(IAuthentication).FullName, typeof(PassDummyAuth).FullName);
        }
    }
}
