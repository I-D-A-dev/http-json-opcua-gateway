using System.Text.Json;

public sealed class SensorApiPoller(HttpClient httpClient, SensorValueStore store, GatewayOptions options)
{
    private long _pollCount;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, options.PollingIntervalSeconds));

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var response = await httpClient.GetAsync(options.ApiUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                var values = AddGatewayStatus(
                    JsonSensorFlattener.Flatten(document.RootElement),
                    success: true,
                    errorMessage: "",
                    httpStatusCode: (int)response.StatusCode);

                store.Update(values);
                Console.WriteLine($"{DateTimeOffset.Now:O} updated {values.Count} sensor value(s).");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                store.Update(AddGatewayStatus(
                    new Dictionary<string, SensorValue>(StringComparer.Ordinal),
                    success: false,
                    errorMessage: ex.Message,
                    httpStatusCode: 0));
                Console.Error.WriteLine($"{DateTimeOffset.Now:O} API polling failed: {ex.Message}");
            }

            try
            {
                await Task.Delay(interval, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private IReadOnlyDictionary<string, SensorValue> AddGatewayStatus(
        IReadOnlyDictionary<string, SensorValue> values,
        bool success,
        string errorMessage,
        int httpStatusCode)
    {
        var updatedValues = new Dictionary<string, SensorValue>(values, StringComparer.Ordinal)
        {
            ["gateway.poll_count"] = new SensorValue(Interlocked.Increment(ref _pollCount), Opc.Ua.DataTypeIds.Int64),
            ["gateway.last_poll_utc"] = new SensorValue(DateTime.UtcNow, Opc.Ua.DataTypeIds.DateTime),
            ["gateway.last_poll_success"] = new SensorValue(success, Opc.Ua.DataTypeIds.Boolean),
            ["gateway.last_http_status"] = new SensorValue((long)httpStatusCode, Opc.Ua.DataTypeIds.Int64),
            ["gateway.last_error"] = new SensorValue(errorMessage, Opc.Ua.DataTypeIds.String)
        };

        return updatedValues;
    }
}
