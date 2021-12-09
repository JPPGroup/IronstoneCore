using System;

namespace Jpp.Ironstone.Core
{
    internal class HelloWordStructure
    {
        public string Generated => DateTime.Now.ToString();
        public bool RunningOnForge => CoreExtensionApplication.ForgeDesignAutomation;
    }
}
