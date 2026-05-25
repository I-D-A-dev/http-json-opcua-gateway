# Sensor HTTP to OPC UA Gateway

WebAPIからJSON形式のセンサ値をHTTP GETで取得し、OPC UAサーバーとして公開する .NET コンソールアプリです。

## 使い方

1. `appsettings.json` の `ApiUrl` を実際のWebAPI URLへ変更します。
2. アプリを起動します。

```powershell
dotnet run
```

OPC UAクライアントから次のエンドポイントへ接続します。

```text
opc.tcp://<このPCのIP>:4840/SensorGateway
```

`Objects/Sensors` 配下に、JSONパスと同じ名前の変数ノードが作成されます。例えば `{"device":{"temperature":24.5}}` は `device.temperature` として公開されます。

`gateway.poll_count`、`gateway.last_poll_utc`、`gateway.last_poll_success` は取得周期ごとに更新されます。Open-Meteoの気象値は短時間では変わらないことがあるため、OPC UAクライアント側で更新確認をするときはまず `gateway.poll_count` を見ると分かりやすいです。

## 設定

- `ApiUrl`: センサ値を返すWebAPI URL
- `PollingIntervalSeconds`: HTTP取得周期
- `HttpTimeoutSeconds`: HTTPタイムアウト
- `OpcUaEndpoint`: OPC UAサーバーの公開URL
- `NamespaceUri`: センサノード用のOPC UA名前空間URI

## 動作確認

JSONフラット化の自己テストを実行できます。

```powershell
dotnet run -- --self-test
```

## exe化

Windows x64向けに、.NETランタイム込みの単一exeを作成できます。

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish\win-x64
```

出力先は `publish\win-x64` です。`SensorHttpOpcUaGateway.exe` と同じフォルダにある `appsettings.json` を編集してから起動してください。
