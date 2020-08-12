using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Unity;

namespace Jpp.Ironstone.Core
{
    /// <summary>
    /// Extends Extension Application interface to provide methods for core to use
    /// </summary>
    public interface IIronstoneExtensionApplication : IExtensionApplication
    {
        void InjectContainer(IUnityContainer container);

        void CreateUI();
    }
}
