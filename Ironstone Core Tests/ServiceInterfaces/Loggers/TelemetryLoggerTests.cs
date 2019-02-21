using System;
using System.Linq;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.ServiceInterfaces.Loggers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces.Loggers
{
    [TestFixture]
    class TelemetryLoggerTests
    {
        [Test]
        public void LogEntry()
        {
            // Arrange
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger(client);
            
            // Act
            logger.Entry("message");
            
            // Assert
            var telemetryCount = mockChannel.SentTelemetries.Count;
            var traceCount = mockChannel.SentTraces.Count();

            Assert.AreEqual(1, telemetryCount, "Incorrect number of telemetries sent.");
            Assert.AreEqual(1, traceCount, "Incorrect number of traces sent.");
        }

        [Test]
        public void LogEntryWithSeverityInformation()
        {
            // Arrange
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger(client);

            // Act
            logger.Entry("message", Severity.Information);
            
            // Assert
            var telemetryCount = mockChannel.SentTelemetries.Count;
            var traceCount = mockChannel.SentTraces.Count();

            Assert.AreEqual(1, telemetryCount, "Incorrect number of telemetries sent.");
            Assert.AreEqual(1, traceCount, "Incorrect number of traces sent.");
        }

        [Test]
        public void LogEntryWithSeverityDebug()
        {
            // Arrange
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger(client);

            // Act
            logger.Entry("message", Severity.Debug);
            
            // Assert
            var telemetryCount = mockChannel.SentTelemetries.Count;
            var traceCount = mockChannel.SentTraces.Count();

            Assert.AreEqual(1, telemetryCount, "Incorrect number of telemetries sent.");
            Assert.AreEqual(1, traceCount, "Incorrect number of traces sent.");
        }

        [Test]
        public void LogEntryWithSeverityWarning()
        {
            // Arrange
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger(client);

            // Act
            logger.Entry("message", Severity.Warning);
            
            // Assert
            var telemetryCount = mockChannel.SentTelemetries.Count;
            var traceCount = mockChannel.SentTraces.Count();

            Assert.AreEqual(1, telemetryCount, "Incorrect number of telemetries sent.");
            Assert.AreEqual(1, traceCount, "Incorrect number of traces sent.");
        }

        [Test]
        public void LogEntryWithSeverityError()
        {
            // Arrange
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger(client);

            // Act
            logger.Entry("message", Severity.Error);
            
            // Assert
            var telemetryCount = mockChannel.SentTelemetries.Count;
            var traceCount = mockChannel.SentTraces.Count();

            Assert.AreEqual(1, telemetryCount, "Incorrect number of telemetries sent.");
            Assert.AreEqual(1, traceCount, "Incorrect number of traces sent.");
        }

        [Test]
        public void LogEntryWithSeverityCrash()
        {
            // Arrange
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger(client);

            // Act
            logger.Entry("message", Severity.Crash);
            
            // Assert
            var telemetryCount = mockChannel.SentTelemetries.Count;
            var traceCount = mockChannel.SentTraces.Count();

            Assert.AreEqual(1, telemetryCount, "Incorrect number of telemetries sent.");
            Assert.AreEqual(1, traceCount, "Incorrect number of traces sent.");
        }

        [Test]
        public void LogEvent()
        {
            // Arrange
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger(client);

            // Act
            logger.LogEvent(Event.Command, "parameters");
            
            // Assert
            var telemetryCount = mockChannel.SentTelemetries.Count;
            var eventCount = mockChannel.SentEvents.Count();

            Assert.AreEqual(1, telemetryCount, "Incorrect number of telemetries sent.");
            Assert.AreEqual(1, eventCount, "Incorrect number of events sent.");
        }

        [Test]
        public void LogException()
        {
            // Arrange
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger(client);

            // Act
            logger.LogException(new Exception("exception"));
            
            // Assert
            var telemetryCount = mockChannel.SentTelemetries.Count;
            var exceptionCount = mockChannel.SentExceptions.Count();

            Assert.AreEqual(1, telemetryCount, "Incorrect number of telemetries sent.");
            Assert.AreEqual(1, exceptionCount, "Incorrect number of exceptions sent.");
        }
    }
}
