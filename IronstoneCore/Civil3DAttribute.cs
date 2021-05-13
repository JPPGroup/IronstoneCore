﻿using AspectInjector.Broker;
using System;
using System.Linq;
using Jpp.Ironstone.Core.Properties;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Core
{
    [Injection(typeof(Civil3DAspect))]
    [AttributeUsage(AttributeTargets.Method)]
    public class Civil3DAttribute : Attribute
    {
        public bool TagDrawing { get; }

        public Civil3DAttribute(bool tagDrawing = true)
        {
            TagDrawing = tagDrawing;
        }
    }

    [Aspect(Scope.Global, Factory = typeof(Civil3DAspectFactory))]
    public class Civil3DAspect
    {
        private ILogger<Civil3DAspect> _logger;
        private ICivil3dController _controller;

        public Civil3DAspect(ILogger<Civil3DAspect> logger, ICivil3dController controller)
        {
            _logger = logger;
            _controller = controller;
        }

        [Advice(Kind.Around, Targets = Target.Method)]
        public object HandleMethod([Argument(Source.Arguments)] object[] arguments,
            [Argument(Source.Target)] Func<object[], object> method, [Argument(Source.Triggers)] Attribute[] triggers)
        {
            bool tagDrawing = triggers.OfType<Civil3DAttribute>().Any(x => x.TagDrawing);

            if (CoreExtensionApplication.Civil3D)
            {
                var result = method(arguments);
                if(tagDrawing)
                    _controller.MarkCurrentDrawingAsCivil3D();
                return result;
            }
            else
            {
                _logger.LogInformation(Resources.Civil3DAttribute_Inform_Not);
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
            Civil3DAspect aspect = CoreExtensionApplication._current.Container.GetRequiredService<Civil3DAspect>();

            return aspect;
        }
    }
}
