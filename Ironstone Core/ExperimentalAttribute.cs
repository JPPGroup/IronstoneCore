using AspectInjector.Broker;
using System;
using System.Diagnostics;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core.Properties;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Unity;

namespace Jpp.Ironstone.Core
{
    [Injection(typeof(ExperimentalAspect))]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ExperimentalAttribute : Attribute
    {
    }

    [Aspect(Scope.Global, Factory = typeof(ExperimentalAspectFactory))]
    public class ExperimentalAspect
    {
        private ILogger _logger;
        private IUserSettings _settings;

        public ExperimentalAspect(ILogger logger, IUserSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        [Advice(Kind.Around, Targets = Target.Method)]
        public object HandleMethod([Argument(Source.Arguments)] object[] arguments, [Argument(Source.Type)] Type owner,
            [Argument(Source.Target)] Func<object[], object> method, [Argument(Source.Name)] string methodName)
        {
            var rtMethod = owner.GetRuntimeMethod(methodName, new Type[] { });
            var attribute = rtMethod.GetCustomAttribute<CommandMethodAttribute>();

            if (attribute == null)
            {
                _logger.LogEvent(Event.Message, Resources.LogCommand_Inform_Not);
                return null;
            }

            bool? experimentalEnabled = _settings.GetValue<bool>($"experimental.{owner.Assembly.GetName().Name}.{attribute.GlobalName}");

            if (experimentalEnabled.HasValue && experimentalEnabled.Value)
            {
                var result = method(arguments);
                return result;
            }
            else
            {
                _logger.LogEvent(Event.Message, Resources.LogCommand_Inform_Experimental);
                return null;
            }
        }
    }

    public static class ExperimentalAspectFactory
    {
        public static object GetInstance(Type type)
        {
            if (type != typeof(ExperimentalAspect))
            {
                throw new ArgumentException($"{nameof(ExperimentalAspectFactory)} can create instances only of type {nameof(ExperimentalAspect)}");
            }

            //Realise this is an Anti-Pattern but this is better than remembering to set properties on the factory at runtime
            ExperimentalAspect aspect = CoreExtensionApplication._current.Container.Resolve<ExperimentalAspect>();

            return aspect;
        }
    }
}
