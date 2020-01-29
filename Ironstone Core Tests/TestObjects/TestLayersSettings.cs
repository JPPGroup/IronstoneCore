using Jpp.Ironstone.Core.ServiceInterfaces;
using System;

namespace Jpp.Ironstone.Core.Tests.TestObjects
{
    public class TestLayersEnabledSettings : IUserSettings
    {
        public IUserSettings LoadFrom(string path)
        {
            throw new NotImplementedException();
        }

        public string GetValue(string key)
        {
            if (key == "layers-unlock-unfreeze") return true.ToString();
            if (key == "layers-switch-on") return true.ToString();

            return null;
        }

        public T? GetValue<T>(string key) where T : struct
        {
            throw new NotImplementedException();
        }
    }

    public class TestLayersDisabledSettings : IUserSettings
    {
        public IUserSettings LoadFrom(string path)
        {
            throw new NotImplementedException();
        }

        public string GetValue(string key)
        {
            if (key == "layers-unlock-unfreeze") return false.ToString();
            if (key == "layers-switch-on") return false.ToString();

            return null;
        }

        public T? GetValue<T>(string key) where T : struct
        {
            throw new NotImplementedException();
        }
    }
}
