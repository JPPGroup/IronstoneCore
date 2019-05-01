using System;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces.Loggers
{
    [Serializable]
    public class TelemetryClientTestData
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public string OperatingSystem { get; set; }
        public string AcVersion { get; set; }
        public string CoreVersion { get; set; }
    }
}
