using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using AspectInjector.Broker;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.Mocking;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests
{
    [TestFixture]
    public class CoreExtensionApplicationTests : IronstoneTestFixture
    {
        public CoreExtensionApplicationTests() : base(Assembly.GetExecutingAssembly(), typeof(CoreExtensionApplicationTests)) { }

        /*[Test]
        public void VerifyDefaultResolvers()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyDefaultResolversResident)));
        }

        public bool VerifyDefaultResolversResident()
        {
            bool auth = CoreExtensionApplication._current.Container.GetRequiredService<IAuthentication>() is PassDummyAuth;
            bool logger = CoreExtensionApplication._current.Container.GetRequiredService<ILogger>() is CollectionLogger;
            bool moduleLoader = CoreExtensionApplication._current.Container.GetRequiredService<IModuleLoader>() is ModuleLoader;

            return (auth && logger && moduleLoader);
        }

        [Test]
        public void VerifyConfigurationResolver()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyConfigurationResolverResident)));
        }

        public bool VerifyConfigurationResolverResident()
        {            
            Configuration config = CoreExtensionApplication._current.Container.GetRequiredService<Configuration>();
            return (config.LogFileRelative.Equals("UnitTestsIronstone.Log"));
        }*/
        [Test]
        public void RunHelloWorld()
        {
            bool result = RunTest<bool>(nameof(RunHelloWorldResident));
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "helloworld.json");
            Assert.True(File.Exists(path));
        }

        public bool RunHelloWorldResident()
        {
            CoreExtensionApplication.HelloWorld();
            return true;
        }

        [Test]
        public void IsForgeDesignAutomation()
        {            
            Assert.False(RunTest<bool>(nameof(IsForgeDesignAutomationResident)));
        }

        public bool IsForgeDesignAutomationResident()
        {            
            return CoreExtensionApplication.ForgeDesignAutomation;
        }
    }
}
