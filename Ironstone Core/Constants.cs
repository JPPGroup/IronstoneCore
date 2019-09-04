namespace Jpp.Ironstone.Core
{
    public static class Constants
    {
        public const string IRONSTONE_CONCEPT_TAB_ID = "IRONSTONE_CONCEPT";
        public const string IRONSTONE_TAB_ID = "IRONSTONE_DESIGN";
        public const string REG_APP_NAME = "JPPI";

        public const string TEMPLATE_MASTER_KEY = "JPPTemplateMasterID";
        public const string TEMPLATE_SLAVE_KEY = "JPPTemplateSlaveID";

        /// <summary>
        /// All valid document extensions, used as part of re-wiring the stores on SaveCompleted.
        /// NB: All extensions in lower case.
        /// </summary>
        public static readonly string[] VALID_DOCUMENT_EXTENSIONS = { ".dwg", ".dwt", ".dxf" };
    }
}
