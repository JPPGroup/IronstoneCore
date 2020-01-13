using System;
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
                try
                {
                    using (StreamReader sr = File.OpenText(path))
                    {
                        string json = sr.ReadToEnd().ToLower();
                        try
                        {
                            newData = JObject.Parse(json);
                        }
                        catch (JsonReaderException ex)
                        {
                            _logger.Entry($"Invalid settings file found at path {path}", Severity.Error);
                            _logger.LogException(ex);
                            return this;
                        }
                    }

                    JsonMergeSettings mergeSettings = new JsonMergeSettings()
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    };

                    _jObject.Merge(newData, mergeSettings);
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.Entry($"Access denied to settings file at path {path}", Severity.Error);
                }
            }
            else
            {
                _logger.Entry($"No settings file found at path {path}", Severity.Warning);
            }

            return this;
        }

        public T? GetValue<T>(string key) where T : struct
        {
            var root = GetNode(key);

            // If this cast fails, this will fail and throw an exception. Considered intentional; if setting is found
            // but unable to cast, then that setting is invalid. No default value should be assumed.
            return root?.Value<T>();
        }

        public string GetValue(string key)
        {
            var root = GetNode(key);

            // If this cast fails (i.e. calling a setting group rather a setting) this will fail and throw an exception
            // Considered intentional so that changing a setting to a group down the line will not change a failure silently
            // to a default value.
            return root?.Value<string>();
        }

        private JToken GetNode(string key)
        {
            string[] path = key.ToLower().Split('.');
            JToken root = _jObject;
            foreach (string s in path)
            {
                root = root[s];
                if (root == null)
                {
                    _logger.Entry($"Invalid settings path {key}. Key {s} not found.", Severity.Warning);
                    return null;
                }
            }

            return root;
        }
    }
}
