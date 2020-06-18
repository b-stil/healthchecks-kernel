using System;
using System.Collections.Generic;

namespace HealthChecks.Kernel
{
    public class HealthStatusResponse
    {
        public string Status { get; set; }

        public TimeSpan TotalDuration { get; set; }

        public IList<HealthCheckEntryResult> Results { get; set; }
    }

    public class HealthCheckEntryResult
    {
        public string HealthCheck { get; set; }

        public string Status { get; set; }

        public string Description { get; set; }

        public TimeSpan Duration { get; set; }

        public HealthCheckException Exception { get; set; }

        public IDictionary<string, object> Details { get; set; }
    }

    public class HealthCheckException
    {
        public string Message { get; set; }

        public string Detail { get; set; }

        public HealthCheckException(string message, string detail)
        {
            Message = message;
            Detail = detail;
        }
    }
}
