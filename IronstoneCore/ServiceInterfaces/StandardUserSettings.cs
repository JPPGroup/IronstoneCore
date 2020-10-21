using System;
using System.Globalization;
using System.IO;
using System.Reflection;
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
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jObject = new JObject();
            
            this.LoadStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Jpp.Ironstone.Core.Resources.BaseConfig.json"))
                .LoadFrom(configuration.NetworkUserSettingsPath).LoadFrom(configuration.UserSettingsPath);
        }

        public IUserSettings LoadFrom(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (Stream stream = File.Open(path, FileMode.Open))
                    {
                        return LoadStream(stream);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.Entry($"Access denied to settings file at path {path}", Severity.Error);
                    return this;
                }
                catch (JsonReaderException ex)
                {
                    _logger.Entry($"Invalid settings file found at path {path}", Severity.Error);
                    _logger.LogException(ex);
                    return this;
                }
            }
            else
            {
                _logger.Entry($"No settings file found at path {path}", Severity.Warning);
                return this;
            }
        }

        public IUserSettings LoadStream(Stream stream)
        {
            JObject newData;
            using (StreamReader sr = new StreamReader(stream))
            {
                //TODO: Verify why cast to lower?
                newData = JObject.Parse(sr.ReadToEnd().ToLower(CultureInfo.InvariantCulture));
            }

            JsonMergeSettings mergeSettings = new JsonMergeSettings()
            {
                MergeArrayHandling = MergeArrayHandling.Union
            };

            _jObject.Merge(newData, mergeSettings);

            return this;
        }

        public T? GetValue<T>(string key) where T : struct
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            
            var root = GetNode(key);

            // If this cast fails, this will fail and throw an exception. Considered intentional; if setting is found
            // but unable to cast, then that setting is invalid. No default value should be assumed.
            return root?.Value<T>();
        }

        public string GetValue(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var root = GetNode(key);

            // If this cast fails (i.e. calling a setting group rather a setting) this will fail and throw an exception
            // Considered intentional so that changing a setting to a group down the line will not change a failure silently
            // to a default value.
            return root?.Value<string>();
        }

        public T GetObject<T>(string key) where T : class
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var root = GetNode(key);
            return root.ToObject<T>();
        }

        private JToken GetNode(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

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
