public sealed class SensorValueStore
{
    private readonly object _lock = new();
    private readonly Dictionary<string, SensorValue> _latest = new(StringComparer.Ordinal);

    public event Action<IReadOnlyDictionary<string, SensorValue>>? ValuesUpdated;

    public IReadOnlyDictionary<string, SensorValue> Snapshot()
    {
        lock (_lock)
        {
            return new Dictionary<string, SensorValue>(_latest, StringComparer.Ordinal);
        }
    }

    public void Update(IReadOnlyDictionary<string, SensorValue> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        lock (_lock)
        {
            foreach (var item in values)
            {
                _latest[item.Key] = item.Value;
            }
        }

        ValuesUpdated?.Invoke(values);
    }
}
