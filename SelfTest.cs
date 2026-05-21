using System.Text.Json;
using Opc.Ua;

public static class SelfTest
{
    public static void Run()
    {
        const string sampleJson = """
        {
          "temperature": 24.5,
          "running": true,
          "device": {
            "name": "line-1",
            "pressure": 101
          },
          "channels": [
            { "value": 10 },
            { "value": 11.25 },
            null
          ]
        }
        """;

        using var document = JsonDocument.Parse(sampleJson);
        var values = JsonSensorFlattener.Flatten(document.RootElement);

        Require(values.Count == 6, "expected six flattened values");
        Require(values["temperature"].DataType == DataTypeIds.Double, "temperature should be Double");
        Require(values["running"].DataType == DataTypeIds.Boolean, "running should be Boolean");
        Require(values["device.name"].DataType == DataTypeIds.String, "device.name should be String");
        Require(values["device.pressure"].DataType == DataTypeIds.Int64, "device.pressure should be Int64");
        Require(values["channels.0.value"].Value.Equals(10L), "channels.0.value should be 10");
        Require(values["channels.1.value"].Value.Equals(11.25), "channels.1.value should be 11.25");

        Console.WriteLine("Self-test passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Self-test failed: {message}");
        }
    }
}
