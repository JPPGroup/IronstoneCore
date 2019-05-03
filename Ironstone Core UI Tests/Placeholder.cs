using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Ironstone.Core.UI.Tests.Properties;
using Jpp.Ironstone.Core.UI.Views;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Jpp.Ironstone.Core.UI.Tests
{
    [TestFixture()]
    class Placeholder
    {
        [Test]
        public void PlaceholderForCodeCoverage()
        {
            UIHelper.LoadImage(Resources.About);
            Assert.Pass("Valid");
        }
    }
}
