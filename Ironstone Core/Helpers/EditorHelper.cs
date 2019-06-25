using System;
using System.Net.NetworkInformation;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.Helpers
{
    public static class EditorHelper
    {
        public static PromptDoubleResult PromptForDouble(string promptMessage, double defaultValue = 0.0)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var doubleOptions = new PromptDoubleOptions(promptMessage);

            if (Math.Abs(defaultValue - 0.0) > double.Epsilon)
            {
                doubleOptions.UseDefaultValue = true;
                doubleOptions.DefaultValue = defaultValue;
            }

            var promptDoubleResult = ed.GetDouble(doubleOptions);

            return promptDoubleResult;
        }

        public static PromptPointResult PromptForPosition(string promptMessage)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var pPtOpts = new PromptPointOptions(promptMessage);

            return ed.GetPoint(pPtOpts);
        }

        public static PromptSelectionResult PromptForSelection(string promptMessage, SelectionFilter filter = null)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var selectionOptions = new PromptSelectionOptions { MessageForAdding = promptMessage };
            var selectionResult = ed.GetSelection(selectionOptions, filter);

            return selectionResult;
        }

        public static PromptResult PromptForKeywords(string promptMessage, string[] keywords) 
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var pKeyOpts = new PromptKeywordOptions(promptMessage);

            foreach (var keyword in keywords)
            {
                pKeyOpts.Keywords.Add(keyword);
            }
            
            pKeyOpts.AllowNone = false;

           return ed.GetKeywords(pKeyOpts);
        }


        public static void WriteMessage(string message)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(message);
        }
    }
}
