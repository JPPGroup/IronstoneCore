using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Unity;

namespace Jpp.Ironstone.Core
{
    /// <summary>
    /// Extends Extension Application interface to provide methods for core to use
    /// </summary>
    public interface IIronstoneExtensionApplication : IExtensionApplication
    {
        void RegisterServices(IServiceCollection container);

        void InjectContainer(IServiceProvider provider);

        void CreateUI();
    }
}
