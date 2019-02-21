using System;
using System.Linq;
using System.Reflection;
using Jpp.AcTestFramework;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.ServiceInterfaces.Loggers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces.Loggers
{
    [TestFixture]
    class TelemetryLoggerTests : BaseNUnitTestFixture
    {
        public TelemetryLoggerTests() : base(Assembly.GetExecutingAssembly(), typeof(TelemetryLoggerTests)) { }

        [Test]
        public void VerifyDefaultConstructor()
        {
            var result = RunTest<TelemetryClientTestData>("VerifyDefaultConstructorResident");

            Assert.False(string.IsNullOrEmpty(result.SessionId),"Invalid session id.");
            Assert.False(string.IsNullOrEmpty(result.UserId), "Invalid user id.");
            Assert.False(string.IsNullOrEmpty(result.OperatingSystem), "Invalid operating system.");
            Assert.False(string.IsNullOrEmpty(result.AcVersion), "Invalid ac version.");
            Assert.False(string.IsNullOrEmpty(result.CoreVersion), "Invalid core version.");
        }

        public TelemetryClientTestData VerifyDefaultConstructorResident()
        {
            var logger = new TelemetryLogger();

            return new TelemetryClientTestData
            {
                SessionId = logger.Client.Context.Session.Id,
                UserId = logger.Client.Context.User.Id,
                OperatingSystem = logger.Client.Context.Device.OperatingSystem,
                AcVersion = logger.Client.Context.GlobalProperties["AcVersion"],
                CoreVersion = logger.Client.Context.GlobalProperties["CoreVersion"]
            };
        }

        [Test]
        public void VerifyLogEntry()
        {
            var result = RunTest<int>("VerifyLogEntryResident");
            Assert.AreEqual(1, result, "Incorrect number of traces sent.");
        }

        public int VerifyLogEntryResident()
        {
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger {Client = client};

            logger.Entry("message");

            return mockChannel.SentTraces.Count();
        }

        [Test]
        public void VerifyLogEntryWithSeverityInformation()
        {
            var result = RunTest<int>("VerifyLogEntryWithSeverityInformationResident");
            Assert.AreEqual(1, result, "Incorrect number of traces sent.");
        }

        public int VerifyLogEntryWithSeverityInformationResident()
        {
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger { Client = client };

            logger.Entry("message", Severity.Information);

            return mockChannel.SentTraces.Count();
        }

        [Test]
        public void VerifyLogEntryWithSeverityDebug()
        {
            var result = RunTest<int>("VerifyLogEntryWithSeverityDebugResident");
            Assert.AreEqual(1, result, "Incorrect number of traces sent.");
        }

        public int VerifyLogEntryWithSeverityDebugResident()
        {
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger { Client = client };

            logger.Entry("message", Severity.Debug);

            return mockChannel.SentTraces.Count();
        }

        [Test]
        public void VerifyLogEntryWithSeverityWarning()
        {
            var result = RunTest<int>("VerifyLogEntryWithSeverityWarningResident");
            Assert.AreEqual(1, result, "Incorrect number of traces sent.");
        }

        public int VerifyLogEntryWithSeverityWarningResident()
        {
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger { Client = client };

            logger.Entry("message", Severity.Warning);

            return mockChannel.SentTraces.Count();
        }

        [Test]
        public void VerifyLogEntryWithSeverityError()
        {
            var result = RunTest<int>("VerifyLogEntryWithSeverityErrorResident");
            Assert.AreEqual(1, result, "Incorrect number of traces sent.");
        }

        public int VerifyLogEntryWithSeverityErrorResident()
        {
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger { Client = client };

            logger.Entry("message", Severity.Error);

            return mockChannel.SentTraces.Count();
        }

        [Test]
        public void VerifyLogEntryWithSeverityCrash()
        {
            var result = RunTest<int>("VerifyLogEntryWithSeverityCrashResident");
            Assert.AreEqual(1, result, "Incorrect number of traces sent.");
        }

        public int VerifyLogEntryWithSeverityCrashResident()
        {
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger { Client = client };

            logger.Entry("message", Severity.Crash);

            return mockChannel.SentTraces.Count();
        }

        [Test]
        public void VerifyLogEvent()
        {
            var result = RunTest<int>("VerifyLogEventResident");
            Assert.AreEqual(1, result, "Incorrect number of events sent.");
        }

        public int VerifyLogEventResident()
        {
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger { Client = client };

            logger.LogEvent(Event.Command, "parameters");

            return mockChannel.SentEvents.Count();
        }

        [Test]
        public void VerifyLogException()
        {
            var result = RunTest<int>("VerifyLogExceptionResident");
            Assert.AreEqual(1, result, "Incorrect number of exceptions sent.");
        }

        public int VerifyLogExceptionResident()
        {
            var mockChannel = new MockTelemetryChannel();
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = mockChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var client = new TelemetryClient(config);
            var logger = new TelemetryLogger { Client = client };

            logger.LogException(new Exception("exception"));

            return mockChannel.SentExceptions.Count();
        }
    }
}
