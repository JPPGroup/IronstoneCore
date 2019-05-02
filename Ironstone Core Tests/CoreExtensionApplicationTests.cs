using System.Reflection;
using Jpp.Ironstone.Core.Mocking;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.ServiceInterfaces.Loggers;
using NUnit.Framework;
using Unity;

namespace Jpp.Ironstone.Core.Tests
{
    [TestFixture]
    public class CoreExtensionApplicationTests : IronstoneTestFixture
    {
        public CoreExtensionApplicationTests() : base(Assembly.GetExecutingAssembly(), typeof(CoreExtensionApplicationTests)) { }

        [Test]
        public void VerifyDefaultResolvers()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyDefaultResolversResident)));
        }

        public bool VerifyDefaultResolversResident()
        {
            bool auth = CoreExtensionApplication._current.Container.Resolve<IAuthentication>() is PassDummyAuth;
            bool logger = CoreExtensionApplication._current.Container.Resolve<ILogger>() is CollectionLogger;
            bool moduleLoader = CoreExtensionApplication._current.Container.Resolve<IModuleLoader>() is ModuleLoader;

            return (auth && logger && moduleLoader);
        }
    }
}
