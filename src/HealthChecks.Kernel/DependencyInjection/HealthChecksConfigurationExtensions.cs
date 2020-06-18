using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthChecks.Kernel.DependencyInjection
{
    public static class HealthChecksConfigurationExtensions
    {
        /// <summary>
        /// Configure the endpoints for health checks.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="managementPort"></param>
        /// <returns></returns>
        public static IEndpointConventionBuilder ConfigureHealthCheckMappings(this IEndpointRouteBuilder builder, string managementPort)
        {
            builder.MapHealthChecks("/status", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("basic"),
                ResponseWriter = WriteResponse
            }).RequireHost($"*:{managementPort}");

            builder.MapHealthChecks("/status/metrics", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("metrics"),
                ResponseWriter = WriteResponse
            }).RequireHost($"*:{managementPort}");

            return builder.MapHealthChecks("/status/details", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("details"),
                ResponseWriter = WriteResponse
            }).RequireHost($"*:{managementPort}");

        }

        /// <summary>
        /// Formats the response from an <see cref="IHealthCheck"/>.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static Task WriteResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";

            var response = new HealthStatusResponse
            {
                Status = result.Status.ToString(),
                TotalDuration = result.TotalDuration,
                Results = result.Entries.Select(pair => new HealthCheckEntryResult()
                {
                    HealthCheck = pair.Key,
                    Status = pair.Value.Status.ToString(),
                    Description = pair.Value.Description,
                    Duration = pair.Value.Duration,
                    Details = pair.Value.Data as IDictionary<string, object>,
                    Exception = pair.Value.Exception != null ? new HealthCheckException(pair.Value.Exception.Message, pair.Value.Exception.StackTrace) : null
                }).ToList()
            };
            return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(response,
                Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
        }
    }
}
