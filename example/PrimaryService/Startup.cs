using System;
using System.Collections.Generic;
using HealthChecks.Kernel.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrimaryService.Configuration;

namespace PrimaryService
{
    public class Startup
    {
        private const string ConnectionStringKey = "ConnectionString";
        private const string ApiServicesConfigSectionKey = "ApiServices";
        private const string ManagementPortKey = "ManagementPort";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            IHealthChecksBuilder hcBuilder = services.ConfigureHealthChecksBuilder();
            ConfigureApiServices(services, hcBuilder);
            ConfigureDatabases(services, hcBuilder);
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
                endpoints.ConfigureHealthCheckMappings();
            });
        }

        /// <summary>
        /// Configure external API services that should be made available through the <see cref="IHttpClientFactory"/>.
        /// <para>Also configures an <seealso cref="IHealthCheck"/> for the API service.</para>
        /// <example>Configuration
        /// <code>
        /// "ApiServices": [
        ///    {
        ///     "Name": "FakeService",
        ///     "BaseUrl": "https://jsonplaceholder.typicode.com",
        ///     "HealthCheckUrlPath": "todos/1"
        ///    }
        ///  ]
        /// </code>
        /// <para>Name is used to identify the service in the <see cref="IHttpClientFactory"/>.</para>
        /// <para>BaseUrl is used to initialize the HttpClient.</para>
        /// <para>HealthCheckUrlPath is used to run the <see cref="IHealthCheck"/>.</para>
        /// </example>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hcBuilder"></param>
        private void ConfigureApiServices(IServiceCollection services, IHealthChecksBuilder hcBuilder)
        {
            var apiServices = Configuration.GetSection(ApiServicesConfigSectionKey).Get<List<ApiService>>() ?? new List<ApiService>();
            foreach (var api in apiServices)
            {
                if (!String.IsNullOrWhiteSpace(api.Name) && !String.IsNullOrWhiteSpace(api.BaseUrl))
                {
                    services.AddHttpClient(api.Name, c =>
                    {
                        c.BaseAddress = new Uri(api.BaseUrl);
                        c.Timeout = TimeSpan.FromSeconds(15);
                    }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

                    if (!String.IsNullOrWhiteSpace(api.HealthCheckPath))
                    {
                        hcBuilder.AddHttpHealthCheck(api.Name, api.HealthCheckPath);
                    }
                }
            }
        }

        /// <summary>
        /// Configure the databases that should be available to the service.
        /// If ASPNETCORE_CONNECTIONSTRING environment variable is not configured no databases will be added.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hcBuilder"></param>
        private void ConfigureDatabases(IServiceCollection services, IHealthChecksBuilder hcBuilder)
        {
            var connectionString = Configuration[ConnectionStringKey];
            /**
            * Setup Database Stuff
            * **/

            /**
             * Add Database health checks
             * **/
            hcBuilder.AddSqlServerHealthCheck("DefaultConnection", connectionString);
        }
    }
}
