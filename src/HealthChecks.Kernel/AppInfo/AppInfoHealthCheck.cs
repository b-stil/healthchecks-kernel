using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Kernel.AppInfo
{
    /// <summary>
    /// Perform a basic health check that returns some information about the current version of the application.
    /// </summary>
    public class AppInfoHealthCheck : IHealthCheck
    {
        const string Description = "Details for the currently running application";

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var startupAssembly = Assembly.GetEntryAssembly();
            var appInfo = new Dictionary<string, object>
            {
                { "AppName", startupAssembly?.GetName()?.Name },
                { "CreationDate", System.IO.File.GetCreationTime(startupAssembly.Location) },
                { "Version", FileVersionInfo.GetVersionInfo(startupAssembly.Location).ProductVersion },
                { "AspDotnetVersion", $"{startupAssembly?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName} ({System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower()})" },
                { "OsPlatform", System.Runtime.InteropServices.RuntimeInformation.OSDescription },

            };

            return Task.FromResult(HealthCheckResult.Healthy(Description, data: appInfo));
        }
    }
}
