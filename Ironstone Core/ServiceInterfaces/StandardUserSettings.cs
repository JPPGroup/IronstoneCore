using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog.Layouts;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public class StandardUserSettings : IUserSettings
    {
        private ILogger _logger;
        private Dictionary<string, string> _settings;

        public StandardUserSettings(ILogger logger, Configuration configuration)
        {
            _settings = new Dictionary<string, string>();
            this.LoadFrom("N:\\Consulting\\Library\\Ironstone\\Config.json").LoadFrom(configuration.AppData + "Config.json");
            _logger = logger;
        }

        public IUserSettings LoadFrom(string path)
        {
            if (File.Exists(path))
            {
                var importedSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(path);
                foreach (KeyValuePair<string, string> s in importedSettings)
                {
                    if (_settings.ContainsKey(s.Key))
                    {
                        _settings[s.Key] = s.Value;
                    }
                    else
                    {
                        _settings.Add(s.Key, s.Value);
                    }
                }
            }
            else
            {
                _logger.Entry($"No log file found at path {path}", Severity.Warning);
            }

            return this;
        }

        public string GetValue(string key)
        {
            return _settings[key];
        }
    }
}
