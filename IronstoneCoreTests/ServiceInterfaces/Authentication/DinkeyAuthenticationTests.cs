using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.ServiceInterfaces.Authentication;
using Microsoft.Extensions.Logging;
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
            LoggerFactory lf = new LoggerFactory();
            var logger = lf.CreateLogger<IAuthentication>();
            DinkeyAuthentication da = new DinkeyAuthentication(logger);
            return da.GetModuleNameFromPath(path);
        }

        public static IEnumerable VerifyGetModuleNameFromPathTestCases
        {
            get
            {
                yield return new TestCaseData("C:\\test\\path\\NameObjectModel.dll").Returns("NameObjectModel");
                yield return new TestCaseData("C:\\test\\path\\Name.dll").Returns("Name");
                yield return new TestCaseData("C:\\test\\path\\Long NameObjectModel.dll").Returns("Long NameObjectModel");
            }
        }
    }
}
