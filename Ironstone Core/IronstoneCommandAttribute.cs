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

        [Advice(Kind.Around, Targets = Target.Method)]
        public object HandleMethod([Argument(Source.Arguments)] object[] arguments,
            [Argument(Source.Target)] Func<object[], object> method, [Argument(Source.Triggers)] Attribute[] triggers)
        {
            try
            {
                return method(arguments);
            }
            catch (Exception e)
            {
                _logger.Entry($"Unknown exception: {e.Message}", Severity.Crash);
                return null;
            }
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
            IronstoneCommandAspect aspect = CoreExtensionApplication._current.Container.Resolve<IronstoneCommandAspect>();
            return aspect;
        }
    }
}
