using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.Kernel.Http;
using HealthChecks.Kernel.Data;
using HealthChecks.Kernel.AppInfo;

namespace HealthChecks.Kernel.DependencyInjection
{
    public static class HealthChecksBuilderExtensions
    {
        /// <summary>
        /// Setup the IHealthChecksBuilder with the default App level Health Checks.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IHealthChecksBuilder ConfigureHealthChecksBuilder(this IServiceCollection services)
        {
            //Use AddCheck<T>() to allow DI to resolve contructor parameters.
            var builder = services.AddHealthChecks()
            .AddCheck<AppInfoHealthCheck>("AppInfo", tags: new[] { "basic", "details" })
            .AddCheck<GCInfoHealthCheck>("GCInfo", tags: new[] { "metrics" });

            return builder;
        }

        /// <summary>
        /// Add an Http based <see cref="IHealthCheck"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="name">Name of the <see cref="IHealthCheck"/></param>
        /// <param name="urlPath">Url Path that will be added to the BaseAddress of <c>IHttpClientFactory</c></param>
        /// <returns><see cref="IHealthChecksBuilder"/></returns>
        public static IHealthChecksBuilder AddHttpHealthCheck(this IHealthChecksBuilder builder,
            string name,
            string urlPath)
        {
            builder.AddTypeActivatedCheck<HttpHealthCheck>(name, failureStatus: HealthStatus.Unhealthy, tags: new[] { "details" }, args: urlPath);
            return builder;
        }

        /// <summary>
        /// Add a <see cref="IHealthCheck"/> that queries SQL Server to determine availability.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="name">Name of the <see cref="IHealthCheck"/></param>
        /// <param name="connectionString">Connection String for the Database to query.</param>
        /// <returns><see cref="IHealthChecksBuilder"/></returns>
        public static IHealthChecksBuilder AddSqlServerHealthCheck(this IHealthChecksBuilder builder,
            string name,
            string connectionString)
        {
            builder.AddTypeActivatedCheck<SqlServerHealthCheck>(name, failureStatus: HealthStatus.Unhealthy, tags: new[] { "details" }, args: connectionString);
            return builder;
        }
    }
}
