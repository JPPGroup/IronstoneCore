using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.UI
{
    public static class ContextualTabHelper
    {
        /// <summary>
        /// Helper method to iterate over all items in a manger and verify that all items in the parameter collection are witihin the manager
        /// </summary>
        /// <param name="collection">Collection of ObjectID to check are present</param>
        /// <returns>Return true if all object ids are in manager</returns>
        public static bool SelectionRestrictedToCollection(IEnumerable<DrawingObject> collection)
        {
            PromptSelectionResult promptSelectionResult = Application.DocumentManager.MdiActiveDocument.Editor.SelectImplied();
            if (promptSelectionResult.Status != PromptStatus.OK)
                return false;

            SelectionSet selectionSet = promptSelectionResult.Value;

            foreach (SelectedObject selectedObject in selectionSet)
            {
                if (!collection.Any(obj => obj.BaseObject == selectedObject.ObjectId))
                    return false;
            }

            return true;
        }
    }
}
