using System.Text.Json;

public sealed class GatewayOptions
{
    public string ApiUrl { get; init; } = "http://localhost:5000/sensors";

    public int PollingIntervalSeconds { get; init; } = 1;

    public int HttpTimeoutSeconds { get; init; } = 5;

    public string OpcUaEndpoint { get; init; } = "opc.tcp://0.0.0.0:4840/SensorGateway";

    public string NamespaceUri { get; init; } = "urn:SensorHttpGateway";

    public static GatewayOptions Load(string path)
    {
        if (!File.Exists(path))
        {
            return new GatewayOptions();
        }

        var json = File.ReadAllText(path);
        var options = JsonSerializer.Deserialize<GatewayOptions>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return options ?? new GatewayOptions();
    }
}
