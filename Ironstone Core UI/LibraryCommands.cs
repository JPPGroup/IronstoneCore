using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(LibraryCommands))]
namespace Jpp.Ironstone.Core.UI
{
    public static class LibraryCommands
    {
        [CommandMethod("Core_Lib_FromBlock")]
        [IronstoneCommand]
        public static void BlockToTemplate()
        {
            TypedValue[] acTypValAr = new TypedValue[1];
            acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "INSERT"), 0);
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            PromptSelectionResult acSSPrompt = Application.DocumentManager.MdiActiveDocument.Editor.GetSelection(acSelFtr);

            if (acSSPrompt.Status == PromptStatus.OK)
            {
                if(acSSPrompt.Value.Count > 1)
                    throw new ArgumentOutOfRangeException("More items than expected");

                using (Transaction trans = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    DocumentCollection acDocMgr = Application.DocumentManager;
                    Document newDoc = acDocMgr.Add("");
                    using(DocumentLock lockObj = newDoc.LockDocument())
                    {
                        using (Transaction destinationTrans = newDoc.TransactionManager.StartTransaction())
                        {
                            BlockReference refObj = (BlockReference) trans.GetObject(acSSPrompt.Value[0].ObjectId, OpenMode.ForWrite);
                            BlockRefDrawingObject reference = new BlockRefDrawingObject(refObj);

                            Database source = Application.DocumentManager.MdiActiveDocument.Database;
                            BlockDrawingObject newInsance = reference.GetBlock().TransferToDocument(newDoc);
                            TemplateDrawingObject blockDefinition = newInsance.ConvertToTemplate();

                            destinationTrans.Commit();
                        }
                    }

                    Application.DocumentManager.MdiActiveDocument = newDoc;
                }
            }
        }
    }
}
