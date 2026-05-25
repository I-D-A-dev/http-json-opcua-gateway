# Sensor HTTP to OPC UA Gateway

HTTP GETで取得したJSON形式のセンサ値を、OPC UAサーバーの変数ノードとして公開する .NET コンソールアプリです。

現在のサンプル設定では、Open-Meteoの天気APIを取得し、`Objects/Sensors/current.temperature_2m` などのノードとして公開します。

## 必要なもの

### 実行だけする場合

- Windows x64
- `SensorHttpOpcUaGateway.exe`
- `appsettings.json`
- 外部APIへ接続できるインターネット接続
- OPC UAクライアント
  - 例: UaExpert など

`dotnet publish --self-contained true` で作成したexeには .NET ランタイムが含まれるため、実行先PCに .NET を別途インストールする必要はありません。

### ビルド・exe作成をする場合

- .NET SDK 9
  - 入手先: https://dotnet.microsoft.com/download/dotnet/9.0
- Git
  - 入手先: https://git-scm.com/download/win
- NuGetパッケージ
  - `OPCFoundation.NetStandard.Opc.Ua.Server`
  - `dotnet restore` または `dotnet build` 時に NuGet.org から自動取得されます。

## 入手と配置

### GitHubからソースを取得する場合

```powershell
git clone https://github.com/I-D-A-dev/http-json-opcua-gateway.git
cd http-json-opcua-gateway
```

このPCのようにPowerShellのPATHにGitが通っていない場合は、フルパスで実行できます。

```powershell
& "C:\Program Files\Git\cmd\git.exe" clone https://github.com/I-D-A-dev/http-json-opcua-gateway.git
```

### 実行ファイルとして配布する場合

配布先には、以下を同じフォルダに配置してください。

```text
SensorHttpOpcUaGateway.exe
appsettings.json
```

`SensorHttpOpcUaGateway.pdb` はデバッグ用なので、通常の配布では不要です。

初回起動時に以下のフォルダが自動生成されます。

```text
pki/
logs/
```

`pki/` はOPC UA証明書ストア、`logs/` はOPC UAログ用です。exeと同じフォルダに作られる想定です。

## 設定

`appsettings.json` を編集します。

```json
{
  "ApiUrl": "https://api.open-meteo.com/v1/forecast?latitude=35.6895&longitude=139.6917&current=temperature_2m,relative_humidity_2m,precipitation,wind_speed_10m",
  "PollingIntervalSeconds": 1,
  "HttpTimeoutSeconds": 5,
  "OpcUaEndpoint": "opc.tcp://0.0.0.0:4840/SensorGateway",
  "NamespaceUri": "urn:SensorHttpGateway"
}
```

- `ApiUrl`: JSONを返すWebAPI URL
- `PollingIntervalSeconds`: HTTP取得周期
- `HttpTimeoutSeconds`: HTTPタイムアウト秒数
- `OpcUaEndpoint`: OPC UAサーバーの公開エンドポイント
- `NamespaceUri`: OPC UAノード用の名前空間URI

外部PCのOPC UAクライアントから接続する場合は、WindowsファイアウォールでTCP `4840` の受信を許可してください。

## 実行方法

### ソースから実行

```powershell
dotnet run
```

### exeを実行

```powershell
.\SensorHttpOpcUaGateway.exe
```

OPC UAクライアントから次のエンドポイントへ接続します。

```text
opc.tcp://<このPCのIP>:4840/SensorGateway
```

JSONの値は `Objects/Sensors` 配下に作成されます。例えば `{"device":{"temperature":24.5}}` は `device.temperature` として公開されます。

更新確認には以下の診断ノードを見ると分かりやすいです。

```text
gateway.poll_count
gateway.last_poll_utc
gateway.last_poll_success
gateway.last_http_status
gateway.last_error
```

Open-Meteoの気象値は短時間では変わらないことがあります。`gateway.poll_count` が増えていれば、HTTPポーリングとOPC UAノード更新は動いています。

## 動作確認

JSONフラット化の自己テストを実行できます。

```powershell
dotnet run -- --self-test
```

## exe化

Windows x64向けに、.NETランタイム込みの単一exeを作成します。

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish\win-x64
```

出力先は `publish\win-x64` です。

```text
publish\win-x64\SensorHttpOpcUaGateway.exe
publish\win-x64\appsettings.json
publish\win-x64\SensorHttpOpcUaGateway.pdb
```

配布時は `SensorHttpOpcUaGateway.exe` と `appsettings.json` を同じフォルダに置いてください。
