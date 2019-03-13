using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jpp.AcTestFramework;
using Jpp.Ironstone.Core.Mocking;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.ServiceInterfaces.Authentication;
using Jpp.Ironstone.Core.ServiceInterfaces.Loggers;
using Jpp.Ironstone.Core.Tests.ServiceInterfaces;
using NUnit.Framework;
using Unity;

namespace Jpp.Ironstone.Core.Tests
{
    [TestFixture]
    class CoreExtensionApplicationTests : BaseNUnitTestFixture
    {
        public CoreExtensionApplicationTests() : base(Assembly.GetExecutingAssembly(), typeof(CoreExtensionApplicationTests), "IronstoneCore.dll") { }

        public override void Setup()
        {
            Configuration config = new Configuration();
            config.TestSettings();
            ConfigurationHelper.CreateConfiguration(config);
        }

        [Test]
        public void VerifyDefaultResolvers()
        {
            RunTest<bool>(nameof(VerifyDefaultResolversResident));
        }

        public bool VerifyDefaultResolversResident()
        {
            bool Auth = CoreExtensionApplication._current.Container.Resolve<IAuthentication>() is DinkeyAuthentication;
            bool Logger = CoreExtensionApplication._current.Container.Resolve<ILogger>() is CollectionLogger;
            bool ModuleLoader = CoreExtensionApplication._current.Container.Resolve<IModuleLoader>() is ModuleLoader;

            return (Auth && Logger && ModuleLoader);
        }
    }
}
