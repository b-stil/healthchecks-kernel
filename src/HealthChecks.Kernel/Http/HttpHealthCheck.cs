using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Kernel.Http
{
    /// <summary>
    /// Perform a <see cref="IHealthCheck"/> against a registered <c>IHttpClient</c>.
    /// Initialize with the <see cref="IHealthCheck"/> name as the name registered with the <see cref="IHttpClientFactory"/>.
    /// </summary>
    public class HttpHealthCheck : IHealthCheck
    {
        const string Description = "Availability of an API Service dependency";
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _factory;
        private readonly string _urlPath;

        public HttpHealthCheck(ILogger<HttpHealthCheck> logger, IHttpClientFactory factory, string urlPath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "ApiServiceHealthCheck cannot be performed without a registered ILogger");
            _factory = factory ?? throw new ArgumentNullException(nameof(factory), "ApiServiceHealthCheck cannot be performed without a registered HttpClient");
            _urlPath = urlPath ?? throw new ArgumentNullException(nameof(urlPath), "ApiServiceHealthCheck cannot be performed without a urlPath");
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Running HttpHealthCheck for API Service: {Name}; url: {UrlPath}", context.Registration.Name, _urlPath);
            try
            {
                var client = _factory.CreateClient(context.Registration.Name);
                var response = await client.GetAsync(_urlPath);
                response.EnsureSuccessStatusCode();

                return HealthCheckResult.Healthy(Description,
                    data: new Dictionary<string, object>() { { "response", await response.Content.ReadAsStringAsync().ConfigureAwait(false) } });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error occured while running HttpHealthCheck for API Service: {Name}; with url: {UrlPath}", context.Registration.Name, _urlPath);
                return HealthCheckResult.Unhealthy(Description,
                    exception: ex,
                    data: new Dictionary<string, object>() { { "Error", $"Health Check for API Service: {context.Registration.Name} indicates failure." } });
            }
        }
    }
}
