using Opc.Ua;
using Opc.Ua.Server;

public sealed class SensorGatewayServer(SensorValueStore store, string namespaceUri) : StandardServer
{
    protected override MasterNodeManager CreateMasterNodeManager(
        IServerInternal server,
        ApplicationConfiguration configuration)
    {
        var nodeManagers = new INodeManager[]
        {
            new SensorNodeManager(server, configuration, store, namespaceUri)
        };

        return new MasterNodeManager(server, configuration, null, nodeManagers);
    }
}

public sealed class SensorNodeManager : CustomNodeManager2
{
    private readonly SensorValueStore _store;
    private readonly Dictionary<string, BaseDataVariableState> _variables = new(StringComparer.Ordinal);
    private FolderState? _sensorsFolder;

    public SensorNodeManager(
        IServerInternal server,
        ApplicationConfiguration configuration,
        SensorValueStore store,
        string namespaceUri)
        : base(server, configuration, namespaceUri)
    {
        _store = store;
        SystemContext.NodeIdFactory = this;
        _store.ValuesUpdated += UpsertValues;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _store.ValuesUpdated -= UpsertValues;
        }

        base.Dispose(disposing);
    }

    public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
    {
        lock (Lock)
        {
            _sensorsFolder = new FolderState(null)
            {
                NodeId = new NodeId("Sensors", NamespaceIndex),
                BrowseName = new QualifiedName("Sensors", NamespaceIndex),
                DisplayName = "Sensors",
                TypeDefinitionId = ObjectTypeIds.FolderType,
                EventNotifier = EventNotifiers.None
            };

            _sensorsFolder.AddReference(ReferenceTypes.Organizes, true, ObjectIds.ObjectsFolder);

            if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out var references))
            {
                externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
            }

            references.Add(new NodeStateReference(ReferenceTypes.Organizes, false, _sensorsFolder.NodeId));
            AddPredefinedNode(SystemContext, _sensorsFolder);
            UpsertValues(_store.Snapshot());
        }
    }

    private void UpsertValues(IReadOnlyDictionary<string, SensorValue> values)
    {
        if (_sensorsFolder is null || values.Count == 0)
        {
            return;
        }

        lock (Lock)
        {
            foreach (var item in values)
            {
                if (!_variables.TryGetValue(item.Key, out var variable))
                {
                    variable = CreateVariable(item.Key, item.Value);
                    _sensorsFolder.AddChild(variable);
                    AddPredefinedNode(SystemContext, variable);
                    _variables[item.Key] = variable;
                }
                else if (!Equals(variable.DataType, item.Value.DataType))
                {
                    variable.DataType = item.Value.DataType;
                }

                variable.Value = item.Value.Value;
                variable.StatusCode = StatusCodes.Good;
                variable.Timestamp = DateTime.UtcNow;
                variable.ClearChangeMasks(SystemContext, true);
            }
        }
    }

    private BaseDataVariableState CreateVariable(string path, SensorValue value)
    {
        return new BaseDataVariableState(_sensorsFolder)
        {
            NodeId = new NodeId($"Sensors.{path}", NamespaceIndex),
            BrowseName = new QualifiedName(path, NamespaceIndex),
            DisplayName = path,
            TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
            DataType = value.DataType,
            ValueRank = ValueRanks.Scalar,
            AccessLevel = AccessLevels.CurrentRead,
            UserAccessLevel = AccessLevels.CurrentRead,
            Historizing = false,
            Value = value.Value,
            StatusCode = StatusCodes.Good,
            Timestamp = DateTime.UtcNow
        };
    }
}
