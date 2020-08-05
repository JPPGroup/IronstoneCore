using System;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.ServiceInterfaces.Template;
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

                Document original = Application.DocumentManager.MdiActiveDocument;
                using (Transaction trans = original.TransactionManager.StartTransaction())
                {
                    DocumentCollection acDocMgr = Application.DocumentManager;
                    Document newDoc = acDocMgr.Add("");
                    using(DocumentLock lockObj = newDoc.LockDocument())
                    {
                        using (Transaction destinationTrans = newDoc.TransactionManager.StartTransaction())
                        {
                            BlockReference refObj = (BlockReference) trans.GetObject(acSSPrompt.Value[0].ObjectId, OpenMode.ForWrite);
                            BlockRefDrawingObject reference = new BlockRefDrawingObject(original, refObj);
                            TemplateDrawingObject template = reference.GetBlock().ConvertToTemplate();
                            BlockDrawingObject newBlock = template.TransferToDocument(newDoc);

                            //Iterate through all managers to see if block is referenced anywhere.
                            //If so move to new document
                            foreach (DocumentStore existingStore in DataService.Current.GetExistingStores(original.Name))
                            {
                                foreach (IDrawingObjectManager existingStoreManager in existingStore.Managers)
                                {
                                    foreach (DrawingObject o in existingStoreManager.GetAllDrawingObjects().Where(drawingObject => drawingObject.BaseObject == template.BaseObject))
                                    {
                                        if(o is ITemplatedObject)
                                            ((ITemplatedObject)o).TransferDrawingObject(newDoc, newBlock.BaseObject);
                                    }
                                }
                            }

                            destinationTrans.Commit();
                        }
                    }

                    Application.DocumentManager.MdiActiveDocument = newDoc;
                }
            }
        }

        [CommandMethod("Core_Lib_AddToDrawing")]
        [IronstoneCommand]
        public static void TemplateToBlock()
        {
            ToDrawing(true);
        }

        [CommandMethod("Core_Lib_LoadIntoDrawing")]
        [IronstoneCommand]
        public static void LoadIntoDrawing()
        {
            ToDrawing(false);
        }

        private static void ToDrawing(bool insert)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;

            PromptResult promptResult = editor.GetString("\nEnter block template id: ");
            if (promptResult.Status != PromptStatus.OK)
                return;

            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                Guid id = new Guid(promptResult.StringResult);
                ITemplateSource source = DataService.Current.GetTemplateSource(id);
                (Database database, TemplateDrawingObject template) = source.GetTemplate(id);
                BlockDrawingObject residentTemplateBlock;

                Document sourceDoc = Application.DocumentManager.Open(database.Filename);
                
                using (database)
                using(database.TransactionManager.StartTransaction())
                {
                    residentTemplateBlock = template.TransferToDocument(doc);
                    //template.TransferDrawingObject(doc, residentTemplateBlock.BaseObject);

                    foreach (DocumentStore existingStore in DataService.Current.GetExistingStores(sourceDoc.Name))
                    {
                        foreach (IDrawingObjectManager existingStoreManager in existingStore.Managers)
                        {
                            foreach (DrawingObject o in existingStoreManager.GetAllDrawingObjects().Where(drawingObject => drawingObject.BaseObject == template.BaseObject))
                            {
                                if(o is ITemplatedObject)
                                    ((ITemplatedObject)o).TransferDrawingObject(doc, residentTemplateBlock.BaseObject);
                            }
                        }
                    }
                }

                sourceDoc.CloseAndDiscard();

                if (insert)
                {
                    PromptPointResult result = editor.GetPoint("Please enter location of block:");
                    if (result.Status != PromptStatus.OK)
                        return;

                    BlockRefDrawingObject.Create(doc.Database, result.Value, residentTemplateBlock);
                }

                trans.Commit();
            }
        }
    }
}
