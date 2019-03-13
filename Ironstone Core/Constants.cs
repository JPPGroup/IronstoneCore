using System;

namespace Jpp.Ironstone.Core
{
    public class Constants
    {
        public const string IRONSTONE_TAB_TITLE = "Ironstone";
        public const string IRONSTONE_TAB_ID = "IRONSTONE";

        public static string APPDATA = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JPP Consulting\\Ironstone";

        public const string INSTALLER_URL = "https://ironstone.blob.core.windows.net/ironstone/IronstoneCore.xml";
        public const string BASE_URL = "https://ironstone.blob.core.windows.net/ironstone/";
        public const string OBJECT_MODEL_URL = "https://ironstone.blob.core.windows.net/ironstone/IronstoneObjectModel.xml";
    }
}
