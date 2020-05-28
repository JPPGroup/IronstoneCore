using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.UI.Autocad
{
    public static class EditorExtensions
    {
        public static double? PromptForDouble(this Editor ed, string promptMessage, double defaultValue = 0.0)
        {
            var pDblOpts = new PromptDoubleOptions(promptMessage);

            if (Math.Abs(defaultValue - 0.0) > double.Epsilon)
            {
                pDblOpts.UseDefaultValue = true;
                pDblOpts.DefaultValue = defaultValue;
            }

            var pDblRes = ed.GetDouble(pDblOpts);
            if (pDblRes.Status != PromptStatus.OK) return null;

            return pDblRes.Value;
        }

        public static Point3d? PromptForPosition(this Editor ed, string promptMessage)
        {
            var pPtOpts = new PromptPointOptions(promptMessage);
            var pPtRes = ed.GetPoint(pPtOpts);

            if (pPtRes.Status != PromptStatus.OK) return null;

            return pPtRes.Value;
        }

        public static SelectionSet PromptForSelection(this Editor ed, string promptMessage, SelectionFilter filter = null)
        {
            var pSelOpts = new PromptSelectionOptions { MessageForAdding = promptMessage };
            var pSelRes = ed.GetSelection(pSelOpts, filter);

            return pSelRes.Status != PromptStatus.OK ? null : pSelRes.Value;
        }

        // TODO: CHanges this from an array to collection, as removed need for unnessary casts
        public static string PromptForKeywords(this Editor ed, string promptMessage, string[] keywords, string defaultKeyword = null) 
        {
            var pKeyOpts = new PromptKeywordOptions(promptMessage);

            foreach (var keyword in keywords)
            {
                pKeyOpts.Keywords.Add(keyword);
            }
            
            pKeyOpts.AllowNone = false;
            if (!string.IsNullOrWhiteSpace(defaultKeyword)) pKeyOpts.Keywords.Default = defaultKeyword;

            var pKeyRes = ed.GetKeywords(pKeyOpts);

            return pKeyRes.Status != PromptStatus.Keyword && pKeyRes.Status != PromptStatus.OK ? null : pKeyRes.StringResult;
        }

        public static string PromptForString(this Editor ed, string promptMessage, string defaultValue = null)
        {
            var pStrOpts = new PromptStringOptions(promptMessage) { AllowSpaces = false};
            
            if (!string.IsNullOrWhiteSpace(defaultValue)) pStrOpts.DefaultValue = defaultValue;

            var pStrRes = ed.GetString(pStrOpts);
            return pStrRes.Status != PromptStatus.OK ? null : pStrRes.StringResult;
        }

        public static ObjectId? PromptForEntity(this Editor ed, string promptMessage, Type type, string rejectMessage = null, bool exactMatch = false)
        {
            var pEntPts = new PromptEntityOptions(promptMessage);

            if (!string.IsNullOrWhiteSpace(rejectMessage)) pEntPts.SetRejectMessage(rejectMessage);

            pEntPts.AddAllowedClass(type, exactMatch);

            var pEntRes = ed.GetEntity(pEntPts);
            if (pEntRes.Status != PromptStatus.OK) return null;

            return pEntRes.ObjectId;
        }
    }
}
