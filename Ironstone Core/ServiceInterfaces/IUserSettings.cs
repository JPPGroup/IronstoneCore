using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public interface IUserSettings
    {
        IUserSettings LoadFrom(string path);

        string GetValue(string key);

        T? GetValue<T>(string key) where T : struct;
    }
}
