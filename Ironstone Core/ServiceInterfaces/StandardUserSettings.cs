using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Layouts;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public class StandardUserSettings : IUserSettings
    {
        private ILogger _logger;
        private Dictionary<string, string> _settings;

        public StandardUserSettings(ILogger logger, Configuration configuration)
        {
            _logger = logger;

            _settings = new Dictionary<string, string>();
            this.LoadFrom(configuration.NetworkUserSettingsPath).LoadFrom(configuration.AppData + "Config.json");
        }

        public IUserSettings LoadFrom(string path)
        {
            if (File.Exists(path))
            {
                string json;

                using (StreamReader sr = File.OpenText(path))
                {
                    json = sr.ReadToEnd();
                    try
                    {
                        JToken.Parse(json);
                    }
                    catch (JsonReaderException ex)
                    {
                        _logger.Entry($"Invalid settings file found at path {path}", Severity.Error);
                        return this;
                    }
                }

                var importedSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
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
                _logger.Entry($"No settings file found at path {path}", Severity.Warning);
            }

            return this;
        }

        public string GetValue(string key)
        {
            if (_settings.ContainsKey(key))
            {
                return _settings[key];
            }
            else
            {
                return null;
            }
        }
    }
}
