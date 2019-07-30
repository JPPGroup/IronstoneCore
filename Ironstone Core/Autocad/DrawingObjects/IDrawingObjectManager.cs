using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core.Autocad
{
    public interface IDrawingObjectManager
    {
        /// <summary>
        /// Update any objects marked as dirty. Called by the framework
        /// </summary>
        void UpdateDirty();

        /// <summary>
        /// Update all objects. Called by the framework
        /// </summary>
        void UpdateAll();

        /// <summary>
        /// Removed all objects managed by this instance
        /// </summary>
        void Clear();

        /// <summary>
        /// Mark all objects managed by this instance as dirty
        /// </summary>
        void AllDirty();

        /// <summary>
        /// Activate all objects managed by this instance. Called by the framework upon manager being loaded.
        /// </summary>
        void ActivateObjects();

        /// <summary>
        /// Sets dependencies this drawing object manager
        /// </summary>
        /// <param name="doc">The host document</param>
        /// <param name="log"></param>
        void SetDependencies(Document doc, ILogger log);
    }
}
