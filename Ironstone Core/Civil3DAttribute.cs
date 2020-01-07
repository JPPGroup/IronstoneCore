using AspectInjector.Broker;
using System;
using Jpp.Ironstone.Core.Properties;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Unity;

namespace Jpp.Ironstone.Core
{
    [Injection(typeof(Civil3DAspect))]
    [AttributeUsage(AttributeTargets.Method)]
    class Civil3DAttribute : Attribute
    {
    }

    [Aspect(Scope.Global, Factory = typeof(Civil3DAspectFactory))]
    public class Civil3DAspect
    {
        private ILogger _logger;
        public Civil3DAspect(ILogger logger)
        {
            _logger = logger;
        }

        [Advice(Kind.Around, Targets = Target.Method)]
        public object HandleMethod([Argument(Source.Name)] string name,
            [Argument(Source.Arguments)] object[] arguments,
            [Argument(Source.Target)] Func<object[], object> method)
        {
            if (CoreExtensionApplication.Civil3D)
            {
                var result = method(arguments);
                return result;
            }
            else
            {
                _logger.LogEvent(Event.Message, Resources.Civil3DAttribute_Inform_Not);
                return null;
            }
        }
    }

    public static class Civil3DAspectFactory
    {
        public static object GetInstance(Type type)
        {
            if (type != typeof(Civil3DAspect))
            {
                throw new ArgumentException($"{nameof(Civil3DAspectFactory)} can create instances only of type {nameof(Civil3DAspect)}");
            }

            //Realise this is an Anti-Pattern but this is better than remembering to set properties on the factory at runtime
            ILogger logger = CoreExtensionApplication._current.Container.Resolve<ILogger>();

            if (logger == null)
            {
                throw new ArgumentException($"Logger is null");
            }

            return new Civil3DAspect(logger);
        }
    }
}
