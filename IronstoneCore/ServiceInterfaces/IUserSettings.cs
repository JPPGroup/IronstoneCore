using System.IO;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public interface IUserSettings
    {
        IUserSettings LoadFrom(string path);
        IUserSettings LoadStream(Stream stream);

        string GetValue(string key);
        T? GetValue<T>(string key) where T : struct;
        T GetObject<T>(string key) where T : class;
    }
}
