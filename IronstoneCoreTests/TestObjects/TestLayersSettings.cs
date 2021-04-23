using Jpp.Ironstone.Core.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Jpp.Ironstone.Core.Tests.TestObjects
{
    public class TestLayersEnabledSettings : IConfiguration
    {
        public IConfigurationSection GetSection(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public string this[string key]
        {
            get {
                if (key == "layers-unlock-unfreeze") return true.ToString();
                if (key == "layers-switch-on") return true.ToString();

                return null;
            }
            set { throw new NotImplementedException(); }
        }
    }

    public class TestLayersDisabledSettings : IConfiguration
    {
        public IConfigurationSection GetSection(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public string this[string key]
        {
            get
            {
                if (key == "layers-unlock-unfreeze") return false.ToString();
                if (key == "layers-switch-on") return false.ToString();

                return null;
            }
            set { throw new NotImplementedException(); }
        }
    }
}
