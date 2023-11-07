using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public sealed class AcConsoleLoggerProvider : ILoggerProvider
    {
        private AcConsoleLogger _current;

        public ILogger CreateLogger(string categoryName)
        {
            if (_current == null)
                _current = new AcConsoleLogger();

            return _current;
        }
        public void Dispose()
        {

        }
    }

    public static class AcConsoleLoggerProviderExtensions
    {
        public static ILoggingBuilder AddAcConsoleLogger(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AcConsoleLoggerProvider>());

            return builder;
        }
    }
}
