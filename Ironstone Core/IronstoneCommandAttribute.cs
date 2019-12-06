using System;
using AspectInjector.Broker;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Unity;

namespace Jpp.Ironstone.Core
{
    [Injection(typeof(IronstoneCommandAspect))]
    [AttributeUsage(AttributeTargets.Method)]
    public class IronstoneCommandAttribute : Attribute
    {
    }

    [Aspect(Scope.Global, Factory = typeof(IronstoneCommandAspectFactory))]
    public class IronstoneCommandAspect
    {
        private ILogger _logger;
        public IronstoneCommandAspect(ILogger logger)
        {
            _logger = logger;
        }

        [Advice(Kind.Before)]
        public void LogEnter([Argument(Source.Name)] string name, [Argument(Source.Type)] Type type)
        {
            _logger.LogCommand(type, name);
        }
    }

    public static class IronstoneCommandAspectFactory
    {
        public static object GetInstance(Type type)
        {
            if (type != typeof(IronstoneCommandAspect))
            {
                throw new ArgumentException($"{nameof(IronstoneCommandAspectFactory)} can create instances only of type {nameof(IronstoneCommandAspect)}");
            }

            //Realise this is an Anti-Pattern but this is better than remembering to set properties on the factory at runtime
            ILogger logger = CoreExtensionApplication._current.Container.Resolve<ILogger>();

            if (logger == null)
            {
                throw new ArgumentException($"Logger is null");
            }

            return new IronstoneCommandAspect(logger);
        }
    }
}
