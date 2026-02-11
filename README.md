# Discord .NET Logger Provider
[![NuGet](https://img.shields.io/nuget/dt/discord-serilog-sink.svg?style=flat-square)](https://www.nuget.org/packages/discord-serilog-sink) [![NuGet](https://img.shields.io/nuget/v/discord-serilog-sink.svg?style=flat-square)](https://www.nuget.org/packages/discord-serilog-sink)

A **Serilog** (https://serilog.net/) sink to send log entries to **Discord** (https://discordapp.com/) as message in a channel. 

## Target
[Discord Webhook Client](https://github.com/jlnpinheiro/discord-webhook-client)<br>
.NET 8.0

For more information about suported versions visit https://docs.microsoft.com/pt-br/dotnet/standard/net-standard

## Installation

### NuGet
```
Install-Package discord-serilog-sink
```
### .NET CLI
```
dotnet add package discord-serilog-sink
```
## Configuration
This sample code shows how to add Discord Serilog Sink on a .NET API project:

```csharp
//Program.cs

using Serilog;
using JNogueira.Discord.Serilog;

var myDiscordWebhookUrl = "https://discord.com/api/webhooks/...";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilogWithDiscordSink(myDiscordWebhookUrl)
... 
var app = builder.Build();

Log.Logger = new LoggerConfiguration()
    .WriteToDiscord(app.Services, builder.Configuration)
    .CreateLogger();
```

```json
//appSettings.json

{
  "Serilog": {
    "Discord": {
      "ApplicationName": "Discord Serilog Sink Test",
      "MessageUserName": "Test Bot 1",
      "EnvironmentName": "Development",
      "MinLogEventLevel": "Verbose",
      "MessageEmbedFields": [
        {
          "Name": "Extra info 1",
          "Value": "This is extra info 1."
        },
        {
          "Name": "Extra info 2",
          "Value": "This is extra info 2."
        }
      ]
    }
  }
}
```

## Message types

**Verbose**
```csharp
Log.Logger.Verbose("This is a verbose message from unit test!");
```
![Trace message](https://raw.githubusercontent.com/jlnpinheiro/logger-discord-provider/refs/heads/assets/trace.png?raw=true)

**Debug**
```csharp
Log.Logger.Debug("This is a debug message from unit test!");
```
![Debug message](https://raw.githubusercontent.com/jlnpinheiro/logger-discord-provider/refs/heads/assets/debug.png?raw=true)

**Information**
```csharp
Log.Logger.Information("This is a info message from unit test!");
```
![Debug message](https://raw.githubusercontent.com/jlnpinheiro/logger-discord-provider/refs/heads/assets/information.png?raw=true)

**Warning**
```csharp
Log.Logger.Warning("This is a warning message from unit test!");
```
![Warning message](https://raw.githubusercontent.com/jlnpinheiro/logger-discord-provider/refs/heads/assets/warning.png?raw=true)

**Error**
```csharp
Log.Logger.Error("This is a error message from unit test!");
```
![Error message](https://raw.githubusercontent.com/jlnpinheiro/logger-discord-provider/refs/heads/assets/error.png?raw=true)

**Fatal**
```csharp
Log.Logger.Fatal("This is a fatal message from unit test!");
```
![Error message](https://raw.githubusercontent.com/jlnpinheiro/logger-discord-provider/refs/heads/assets/critical.png?raw=true)

**Handle an exception!**<br/>
The attachment file *"exception-details.txt"* contains more exception details like base exception, stack trace content, exception type, exception extra data information.
```csharp
try
{
    var i = 0;

    var x = 5 / i;
}
catch (Exception ex)
{
    ex.Data["Extra info 1"] = "Extra info 1 value";
    ex.Data["Extra info 2"] = "Extra info 2 value";

    Log.Logger.Error(ex, "An exception occurred while executing the unit test.");
}
```
![Error message](https://raw.githubusercontent.com/jlnpinheiro/logger-discord-provider/refs/heads/assets/exception.png?raw=true)