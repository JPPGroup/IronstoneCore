using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;

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
        /// Set the document that hosts this drawing object manager
        /// </summary>
        /// <param name="doc">The host document</param>
        void SetHostDocument(Document doc);
    }
}
