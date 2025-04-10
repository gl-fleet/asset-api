using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using asset_allocation_api.Config;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace asset_allocation_api.Service.Implementation;

// ReSharper disable once ClassNeverInstantiated.Global
public class HealthCheck(ILogger<HealthCheck> logger) : IHealthCheck
{
    private readonly ILogger<HealthCheck> logger = logger;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using SqlConnection connection = new(AssetAllocationConfig.assetAllocationConnectionString);

            await connection.OpenAsync(cancellationToken);
            SqlCommand command = connection.CreateCommand();
            command.CommandText = "select 1";
            await command.ExecuteNonQueryAsync(cancellationToken);

            return HealthCheckResult.Healthy("PLI request status OK");
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Health check TaskCanceledException : {ex.Message}", ex.Message);
            return HealthCheckResult.Unhealthy("TaskCanceledException exception", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check exception : {ex.Message}", ex.Message);
            return HealthCheckResult.Unhealthy("Unknown exception", ex);
        }
    }

    public static Task WriteListResponse(HttpContext context, HealthReport healthReport)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        JsonWriterOptions options = new() { Indented = true };

        using MemoryStream memoryStream = new();
        using (Utf8JsonWriter jsonWriter = new(memoryStream, options))
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("status", healthReport.Status.ToString());
            jsonWriter.WriteString("pod name", Environment.MachineName);
            jsonWriter.WriteNumber("up time", (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalMilliseconds);
            jsonWriter.WriteStartObject("results");

            foreach (KeyValuePair<string, HealthReportEntry> healthReportEntry in healthReport.Entries)
            {
                jsonWriter.WriteStartObject(healthReportEntry.Key);
                jsonWriter.WriteString("status", healthReportEntry.Value.Status.ToString());
                jsonWriter.WriteString("description", healthReportEntry.Value.Description);
                if (healthReportEntry.Value.Exception != null)
                {
                    jsonWriter.WriteString("Exception message", healthReportEntry.Value.Exception.Message);
                    jsonWriter.WriteString("Exception stacktrace", healthReportEntry.Value.Exception.StackTrace);
                }

                jsonWriter.WriteStartObject("data");

                foreach (KeyValuePair<string, object> item in healthReportEntry.Value.Data)
                {
                    jsonWriter.WritePropertyName(item.Key);

                    JsonSerializer.Serialize(jsonWriter, item.Value, item.Value?.GetType() ?? typeof(object));
                }

                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndObject();
        }

        return context.Response.WriteAsync(Encoding.UTF8.GetString(memoryStream.ToArray()));
    }
}