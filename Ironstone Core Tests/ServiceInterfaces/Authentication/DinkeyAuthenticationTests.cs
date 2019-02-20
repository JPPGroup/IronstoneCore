using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Ironstone.Core.ServiceInterfaces.Authentication;
using Jpp.Ironstone.Core.ServiceInterfaces.Loggers;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces.Authentication
{
    [TestFixture()]
    class DinkeyAuthenticationTests
    {
        [TestCaseSource(nameof(VerifyGetModuleNameFromPathTestCases))]
        public string VerifyGetModuleNameFromPath(string path)
        {
            ConsoleLogger logger = new ConsoleLogger();
            DinkeyAuthentication da = new DinkeyAuthentication(logger);
            return da.GetModuleNameFromPath(path);
        }

        public static IEnumerable VerifyGetModuleNameFromPathTestCases
        {
            get
            {
                yield return new TestCaseData("C:\\test\\path\\Name Objectmodel.dll").Returns("Name Objectmodel");
                yield return new TestCaseData("C:\\test\\path\\Name.dll").Returns("Name");
                yield return new TestCaseData("C:\\test\\path\\Long Name Objectmodel.dll").Returns("Long Name Objectmodel");
            }
        }
    }
}
