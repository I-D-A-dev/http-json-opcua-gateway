using System.Globalization;
using System.Text.Json;
using Opc.Ua;

public static class JsonSensorFlattener
{
    public static IReadOnlyDictionary<string, SensorValue> Flatten(JsonElement root)
    {
        var values = new SortedDictionary<string, SensorValue>(StringComparer.Ordinal);
        Visit(root, "", values);
        return values;
    }

    private static void Visit(JsonElement element, string path, IDictionary<string, SensorValue> values)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    Visit(property.Value, Join(path, property.Name), values);
                }
                break;

            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    Visit(item, Join(path, index.ToString(CultureInfo.InvariantCulture)), values);
                    index++;
                }
                break;

            case JsonValueKind.Number:
                values[DefaultPath(path)] = ToNumberValue(element);
                break;

            case JsonValueKind.String:
                values[DefaultPath(path)] = new SensorValue(element.GetString() ?? "", DataTypeIds.String);
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                values[DefaultPath(path)] = new SensorValue(element.GetBoolean(), DataTypeIds.Boolean);
                break;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                break;
        }
    }

    private static SensorValue ToNumberValue(JsonElement element)
    {
        if (element.TryGetInt64(out var int64Value))
        {
            return new SensorValue(int64Value, DataTypeIds.Int64);
        }

        return new SensorValue(element.GetDouble(), DataTypeIds.Double);
    }

    private static string Join(string prefix, string segment)
    {
        return string.IsNullOrWhiteSpace(prefix) ? segment : $"{prefix}.{segment}";
    }

    private static string DefaultPath(string path)
    {
        return string.IsNullOrWhiteSpace(path) ? "value" : path;
    }
}

public sealed record SensorValue(object Value, NodeId DataType);
