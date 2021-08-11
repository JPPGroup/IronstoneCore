namespace Jpp.Ironstone.Core
{
    /// <summary>
    /// Constant values used throughout the code base
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// String ID for general tab
        /// </summary>
        public const string IronstoneGeneralTabId = "IRONSTONE_GENERAL";
        /// <summary>
        /// String ID for concept tab
        /// </summary>
        public const string IronstoneConceptTabId = "IRONSTONE_CONCEPT";
        /// <summary>
        /// String ID for design tab
        /// </summary>
        public const string IronstoneTabId = "IRONSTONE_DESIGN";
        /// <summary>
        /// Regestered App Name used to access object xdata
        /// </summary>
        public const string RegAppName = "JPPI";

        /// <summary>
        /// All valid document extensions, used as part of re-wiring the stores on SaveCompleted.
        /// NB: All extensions in lower case.
        /// </summary>
        public static readonly string[] ValidDocumentExtensions = { ".dwg", ".dwt", ".dxf" };
    }
}
