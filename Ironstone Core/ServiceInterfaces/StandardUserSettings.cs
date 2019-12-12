using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public class StandardUserSettings : IUserSettings
    {
        private readonly ILogger _logger;
        private readonly JObject _jObject;

        public StandardUserSettings(ILogger logger, Configuration configuration)
        {
            _logger = logger;
            _jObject = new JObject();
            this.LoadFrom(configuration.NetworkUserSettingsPath).LoadFrom(configuration.UserSettingsPath);
        }

        public IUserSettings LoadFrom(string path)
        {
            if (File.Exists(path))
            {
                JObject newData;

                using (StreamReader sr = File.OpenText(path))
                {
                    string json = sr.ReadToEnd();
                    try
                    {
                        newData = JObject.Parse(json);
                    }
                    catch (JsonReaderException ex)
                    {
                        _logger.Entry($"Invalid settings file found at path {path}", Severity.Error);
                        return this;
                    }
                }

                JsonMergeSettings mergeSettings = new JsonMergeSettings()
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                };

                _jObject.Merge(newData, mergeSettings);                
            }
            else
            {
                _logger.Entry($"No settings file found at path {path}", Severity.Warning);
            }

            return this;
        }

        public string GetValue(string key)
        {
            string[] path = key.Split('.');
            JToken root = _jObject;
            foreach(string s in path)
            {
                root = root[s];
                if (root == null)
                    return null;
            }

            return root.Value<string>();
        }
    }
}
