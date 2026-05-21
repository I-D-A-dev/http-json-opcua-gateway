using Opc.Ua;
using Opc.Ua.Configuration;

var options = GatewayOptions.Load("appsettings.json");

if (args.Contains("--self-test", StringComparer.OrdinalIgnoreCase))
{
    SelfTest.Run();
    return;
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cts.Cancel();
};

var store = new SensorValueStore();
var server = new SensorGatewayServer(store, options.NamespaceUri);
var configuration = OpcUaConfigurationFactory.Create(options);
var telemetry = DefaultTelemetry.Create(_ => { });
var application = new ApplicationInstance(configuration, telemetry)
{
    ApplicationName = "SensorHttpOpcUaGateway",
    ApplicationType = ApplicationType.Server
};

await configuration.ValidateAsync(ApplicationType.Server);
await application.CheckApplicationInstanceCertificatesAsync(false, 0, cts.Token);
await application.StartAsync(server);

Console.WriteLine($"OPC UA server started: {options.OpcUaEndpoint}");
Console.WriteLine($"Polling sensor API every {options.PollingIntervalSeconds} second(s): {options.ApiUrl}");
Console.WriteLine("Press Ctrl+C to stop.");

using var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(Math.Max(2, options.HttpTimeoutSeconds))
};

var poller = new SensorApiPoller(httpClient, store, options);
await poller.RunAsync(cts.Token);
