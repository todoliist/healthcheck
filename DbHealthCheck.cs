using System.Data.Common;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class DbHealthCheck : IHealthCheck
{
    private const string DefaultTestQuery = "Select 1";

    public string ConnectionString { get; }

    public string TestQuery { get; }

    public DbHealthCheck(string connectionString)
        : this(connectionString, testQuery: DefaultTestQuery)
    {
    }

    public DbHealthCheck(string connectionString, string testQuery)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        TestQuery = testQuery;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
    {
        using (var connection = new SqlConnection(ConnectionString))
        {
            TelemetryClient tclient = new TelemetryClient();
            var telemetryKey = "ead27f2a-4f8f-46a3-8c5d-ab81fd07fe99";
            TelemetryConfiguration.Active.InstrumentationKey = telemetryKey;
            tclient.InstrumentationKey = telemetryKey;
            try
            {
                await connection.OpenAsync(cancellationToken);

                if (TestQuery != null)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = TestQuery;

                    await command.ExecuteNonQueryAsync(cancellationToken);

                    tclient.TrackEvent(context.Registration.Name, new Dictionary<string, string>
                 {
                     {"HealthCheckMsgs", "DB is Healthy"},
                     {"DB", context.Registration.Name}
                 });
                }
            }
            catch (DbException ex)
            {
                tclient.TrackEvent(context.Registration.Name, new Dictionary<string, string>
                 {
                     {"HealthCheckMsgs", ex.Message},
                     {"DB", context.Registration.Name}
                 });

                return new HealthCheckResult(status: context.Registration.FailureStatus, exception: ex);
            }
        }

        return HealthCheckResult.Healthy();
    }
}