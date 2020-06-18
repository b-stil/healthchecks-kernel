using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Kernel.AppInfo
{
    /// <summary>
    /// Check memory usage by the application and report based on allocation over 1gb.
    /// </summary>
    public class GCInfoHealthCheck : IHealthCheck
    {
        const string Description = "Memory allocation above >= 1gb";

        private readonly long _memoryHighMark = 1024L * 1024L * 1024L;

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var allocated = GC.GetTotalMemory(forceFullCollection: false);
            var allocationDetails = new Dictionary<string, object>()
            {
                { "Allocated", allocated },
                { "Gen0Collections", GC.CollectionCount(0) },
                { "Gen1Collections", GC.CollectionCount(1) },
                { "Gen2Collections", GC.CollectionCount(2) },
            };

            return Task.FromResult(
                    new HealthCheckResult(allocated >= _memoryHighMark ? HealthStatus.Degraded : HealthStatus.Healthy,
                        Description,
                        data: allocationDetails));
        }
    }
}
