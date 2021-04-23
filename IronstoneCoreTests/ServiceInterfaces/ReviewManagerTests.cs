using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Mocking;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.Tests.TestObjects;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces
{
    class ReviewManagerTests : IronstoneTestFixture
    {
        public ReviewManagerTests() : base(Assembly.GetExecutingAssembly(), typeof(ReviewManagerTests))
        {}

        public Configuration Config;

        public override void Setup()
        {
            Config = new Configuration();
            Config.TestSettings();
            ConfigurationHelper.CreateConfiguration(Config);

            //Clear existing log before loading
            if (File.Exists(Config.LogFile))
                File.Delete(Config.LogFile);
        }

        [Test]
        public void VerifyAddition()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyAdditionResident)));
        }

        public bool VerifyAdditionResident()
        {
            try
            {
                Debugger.Launch();
                Document doc = Application.DocumentManager.MdiActiveDocument;
                IReviewManager reviewer = CoreExtensionApplication._current.Container.GetRequiredService<IReviewManager>();
                DataService data = DataService.Current;
                data.PopulateStoreTypes();
                
                int startingItemCount = reviewer.GetUnverified(doc).Count();

                TestDocumentStore testDocumentStore = data.GetStore<TestDocumentStore>(doc.Name);
                TestDrawingObjectManager objectManager = testDocumentStore.GetManager<TestDrawingObjectManager>();

                objectManager.Add(TestDrawingObject.CreateActiveObject(Guid.NewGuid()));
                
                reviewer.Refresh(doc);
                int endItemCount = reviewer.GetUnverified(doc).Count();

                int difference = (endItemCount - startingItemCount);
                return difference == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyValidate()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyValidateResident)));
        }

        public bool VerifyValidateResident()
        {
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                IReviewManager reviewer = CoreExtensionApplication._current.Container.GetRequiredService<IReviewManager>();
                DataService data = DataService.Current;
                data.PopulateStoreTypes();

                int startingItemCount = reviewer.GetUnverified(doc).Count();

                TestDocumentStore testDocumentStore = data.GetStore<TestDocumentStore>(doc.Name);
                TestDrawingObjectManager objectManager = testDocumentStore.GetManager<TestDrawingObjectManager>();

                TestDrawingObject testDrawingObject = TestDrawingObject.CreateActiveObject(Guid.NewGuid());

                objectManager.Add(testDrawingObject);

                reviewer.Refresh(doc);
                int endItemCount = reviewer.GetUnverified(doc).Count();

                int difference = (endItemCount - startingItemCount);
                if (difference != 1)
                    return false;

                reviewer.Verify(doc, testDrawingObject.BaseObjectPtr);
                reviewer.Refresh(doc);
                endItemCount = reviewer.GetUnverified(doc).Count();

                difference = (endItemCount - startingItemCount);

                return difference == 0;

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
