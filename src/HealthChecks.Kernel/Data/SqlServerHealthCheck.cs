using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Kernel.Data
{
    /// <summary>
    /// Perform a Health Check against a SQL Server database.
    /// Initialize with the ConnectionString Key as the <see cref="IHealthCheck"/> name <c>AddCheck<SqlServerHealthCheck>("DefaultConnection");</c>
    /// </summary>
    public class SqlServerHealthCheck : IHealthCheck
    {
        const string Description = "Availability of a database dependency";

        /// <summary>
        /// Query that will be exectued against the database.
        /// </summary>
        const string Query = @"
            SELECT  
                SERVERPROPERTY('Edition') AS Edition
                ,SERVERPROPERTY('ProductVersion') AS Version
                ,SERVERPROPERTY('ProductLevel') AS ProductLevel
                ,SERVERPROPERTY('EngineEdition') AS EngineEdition";

        private readonly ILogger _logger;

        /// <summary>
        /// Database connection string.
        /// Must be provided for the Health Check to execute.
        /// </summary>
        private readonly string _connectionString;

        public SqlServerHealthCheck(ILogger<SqlServerHealthCheck> logger, string connectionString)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "SqlServerHealthCheck cannot be performed without a registered ILogger");
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString), "Unable to perform SqlServerHealthCheck without connection string");
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {

            using (var conn = new SqlConnection(_connectionString))
            {
                _logger.LogInformation("Running SqlServerHealthCheck for Database: {Database}; on Server: {DataSource}", conn.Database, conn.DataSource);
                var queryResults = new Dictionary<string, object>()
                {
                    { "Database", conn.Database },
                    { "Server", conn.DataSource }
                };
                try
                {
                    await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(Query))
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = Query;
                            cmd.CommandType = System.Data.CommandType.Text;

                            using (DbDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                            {
                                if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                                {
                                    for (int colIdx = 0; colIdx < reader.FieldCount; colIdx++)
                                    {
                                        queryResults.Add(reader.GetName(colIdx), reader.IsDBNull(colIdx) ? string.Empty : reader.GetValue(colIdx));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (DbException ex)
                {
                    _logger.LogError(ex, "Error occured during SqlServerHealthCheck for Database: {Database}; on Server: {DataSource}", conn.Database, conn.DataSource);
                    return new HealthCheckResult(
                        context.Registration.FailureStatus,
                        Description,
                        exception: ex,
                        queryResults);
                }

                return HealthCheckResult.Healthy(Description, queryResults);
            }
        }
    }
}
