using System;
using Autodesk.AutoCAD.Runtime;
using Microsoft.Extensions.DependencyInjection;

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
