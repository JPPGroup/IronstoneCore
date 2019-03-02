using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.ServiceInterfaces.Loggers;
using Unity.Lifetime;

namespace Jpp.Ironstone.Core.Mocking
{
    public static class ConfigurationHelper
    {
        public static void CreateConfiguration(Configuration config)
        {
            XmlSerializer xml = new XmlSerializer(typeof(Configuration));
            using (Stream s = File.Open("IronstoneConfig.xml", FileMode.OpenOrCreate))
            {
                xml.Serialize(s, config);
            }
        }
    }
}
