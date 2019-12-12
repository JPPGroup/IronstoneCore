using System;
using System.IO;
using Jpp.Common;
using Jpp.Ironstone.Core.Mocking;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core
{
    public class Configuration
    {
        public SerializableDictionary<string, string> ContainerResolvers { get; set; }

        public bool EnableInstallerUpdate;
        public bool EnableObjectModelUpdate;
        public bool EnableModuleUpdate;
        public bool LoadAppDirectory;
        public string NetworkUserSettingsPath;
        public string UserSettingsPath;

        public string AppData
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + AppDataRelative;
            }
        }
        public string AppDataRelative;

        public string LogFileRelative;
        public string LogFile
        {
            get
            {
                return Path.Combine(AppData, LogFileRelative);
            }
        }

        public string ModuleManifest;
        public string InstallerUrl;
        public string BaseUrl;
        public string ObjectModelUrl;

        public Configuration()
        {
            ContainerResolvers = new SerializableDictionary<string, string>();
            EnableInstallerUpdate = true;
            EnableObjectModelUpdate = true;
            EnableModuleUpdate = true;
            LoadAppDirectory = true;

            AppDataRelative = "\\JPP Consulting\\Ironstone";
            LogFileRelative = "Ironstone.Log";
            ModuleManifest = "ModuleManifest.txt";
            InstallerUrl = "https://ironstone.blob.core.windows.net/ironstone/IronstoneCore.xml";
            BaseUrl = "https://ironstone.blob.core.windows.net/ironstone/";
            ObjectModelUrl = "https://ironstone.blob.core.windows.net/ironstone/IronstoneObjectModel.xml";
            NetworkUserSettingsPath = "N:\\Consulting\\Library\\Ironstone\\Config.json";
            UserSettingsPath = Path.Combine(AppData, "Config.json");

        }

        public void TestSettings()
        {
            EnableInstallerUpdate = false;
            EnableModuleUpdate = false;
            EnableObjectModelUpdate = false;
            LoadAppDirectory = false;
            ContainerResolvers.Add(typeof(IAuthentication).FullName, typeof(PassDummyAuth).FullName);
            LogFileRelative = "UnitTestsIronstone.Log";
            NetworkUserSettingsPath = "BaseConfig.json";
            NetworkUserSettingsPath = "UserConfig.json";
        }
    }
}
