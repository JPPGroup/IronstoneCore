using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace Jpp.Ironstone.Core.Mocking
{
    public static class ConfigurationHelper
    {
        public static void CreateConfiguration(Configuration config)
        {
            var dll = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(dll);
            var configPath = Path.Combine(dir ?? throw new InvalidOperationException(), "IronstoneConfig.xml");
            var xml = new XmlSerializer(typeof(Configuration));

            using (Stream s = File.Open(configPath, FileMode.OpenOrCreate))
            {
                xml.Serialize(s, config);
            }
        }
    }
}
