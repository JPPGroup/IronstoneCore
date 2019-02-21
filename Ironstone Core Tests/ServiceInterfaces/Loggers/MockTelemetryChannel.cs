using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces.Loggers
{
    public class MockTelemetryChannel : ITelemetryChannel
    {
        public ConcurrentBag<ITelemetry> SentTelemetries = new ConcurrentBag<ITelemetry>();
        public IEnumerable<EventTelemetry> SentEvents => GetTelemetries<EventTelemetry>();
        public IEnumerable<ExceptionTelemetry> SentExceptions => GetTelemetries<ExceptionTelemetry>();
        public IEnumerable<TraceTelemetry> SentTraces => GetTelemetries<TraceTelemetry>();

        public bool IsFlushed { get; private set; }
        public bool? DeveloperMode { get; set; }
        public string EndpointAddress { get; set; }
        public void Send(ITelemetry item)
        {
            SentTelemetries.Add(item);
        }
        public void Flush()
        {
            IsFlushed = true;
        }
        public void Dispose()
        {

        }
        private IEnumerable<T> GetTelemetries<T>() where T : ITelemetry
        {
            return SentTelemetries
                .Where(t => t is T)
                .Cast<T>();
        }
    }
}
