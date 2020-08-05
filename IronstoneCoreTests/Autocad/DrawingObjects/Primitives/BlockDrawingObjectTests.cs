using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.Autocad.DrawingObjects.Primitives
{
    // TODO: Consider moving some of these to core to speed runtime
    [TestFixture]
    public class BlockDrawingObjectTests : IronstoneAutocadTestFixture
    {
        public BlockDrawingObjectTests() : base(Assembly.GetExecutingAssembly(), typeof(BlockDrawingObjectTests),
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Test Drawings\\Blocks.dwg")
        { }

        [Test]
        public void ConvertToTemplateValidateKey()
        {
            Assert.IsTrue(RunTest<bool>(nameof(ConvertToTemplateValidateKeyResident)));
        }

        public bool ConvertToTemplateValidateKeyResident()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                BlockDrawingObject testObject = BlockDrawingObject.GetExisting(doc, "ExampleBlock");
                
                //Make sure key doesnt exist just in case file has been accidently saved
                if (testObject.HasKey(BlockDrawingObject.TEMPLATE_ID_KEY))
                    return false;

                testObject.ConvertToTemplate();

                return testObject.HasKey(BlockDrawingObject.TEMPLATE_ID_KEY);
            }
        }
        
        //TODO: Add test to get existing block when not exist

        [Test]
        public void GetExisting()
        {
            Assert.IsTrue(RunTest<bool>(nameof(GetExistingResident)));
        }

        public bool GetExistingResident()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                BlockDrawingObject testObject = BlockDrawingObject.GetExisting(doc, "ExampleBlock");
            }

            return true;
        }

        /// <summary>
        /// Verify that when trying to create a block where a block with that name exists an invalidooperationexception is thrown
        /// </summary>
        [Test]
        public void CreateWhenExisting()
        {
            Assert.IsTrue(RunTest<bool>(nameof(CreateWhenExistingResident)));
        }

        public bool CreateWhenExistingResident()
        {
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (Transaction trans = doc.TransactionManager.StartTransaction())
                {
                    BlockDrawingObject testObject = BlockDrawingObject.Create(doc, "ExampleBlock");
                }

                return false;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
            
        }

        /// <summary>
        /// Verify that when trying to create a block works when a new block name
        /// </summary>
        [Test]
        public void Create()
        {
            Assert.IsTrue(RunTest<bool>(nameof(CreateResident)));
        }

        public bool CreateResident()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                BlockDrawingObject testObject = BlockDrawingObject.Create(doc, "ANewExampleBlock");
            }

            return true;
        }

        /// <summary>
        /// Verify that when trying to create a block works when a new block name
        /// </summary>
        [Test]
        public void TransferToDocument()
        {
            Assert.IsTrue(RunTest<bool>(nameof(TransferToDocumentResident)));
        }

        public bool TransferToDocumentResident()
        {
            Debugger.Launch();
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Document newDoc = Application.DocumentManager.Add("");

            using (DocumentLock lockObj = newDoc.LockDocument())
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            using (Transaction newTrans = newDoc.TransactionManager.StartTransaction())
            {
                BlockDrawingObject testObject = BlockDrawingObject.GetExisting(doc, "ExampleBlock");
                BlockDrawingObject transferBlockDrawingObject = testObject.TransferToDocument(newDoc);

                //Check pointer has been updated
                if (testObject.BaseObjectPtr == transferBlockDrawingObject.BaseObjectPtr)
                    return false;

                //Check object can be retrieved with new pointer
                BlockTableRecord btr = newTrans.GetObject(transferBlockDrawingObject.BaseObject, OpenMode.ForRead) as BlockTableRecord;

                return true;
            }
        }
    }
}

