using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using Serilog.Events;
using System.Security.Claims;

namespace JNogueira.Discord.Serilog.Test;

public class DiscordSerilogSinkTests2
{
    [SetUp]
    public void SetupTest()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("testSettings.json")
            .Build();

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new (ClaimTypes.NameIdentifier, "SomeValueHere"),
                new (ClaimTypes.Name, "gunnar@somecompany.com")
            ])
        );

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(new DefaultHttpContext { User = user });

        var serviceCollection = new ServiceCollection()
            .AddSerilogWithDiscordSink(configuration["Serilog:Discord:WebhookUrl"]);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(serviceProvider);

        var sinkConfig = new DiscordSinkConfiguration
        {
            ApplicationName = "Discord Serilog Sink Test",
            MinLogEventLevel = LogEventLevel.Debug,
            HttpContextAccessor = mockHttpContextAccessor.Object,
            MessageUserName = "Test Bot 2",
            MessageEmbedFields = [
                new("Test Embed Field 1", "This is a test embed field value 1"),
                new("App Version", "1.0.0"),
            ],
            UserClaimValueToDiscordFields = [
                new(ClaimTypes.NameIdentifier, "Name identifier"),
                new(ClaimTypes.Name, "Name")
            ],
            EnvironmentName = configuration["DOTNET_ENVIRONMENT"] ?? configuration["ASPNETCORE_ENVIRONMENT"]
        };

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteToDiscord(mockServiceScope.Object.ServiceProvider, sinkConfig)
            .CreateLogger();
    }

    [Test]
    public async Task Should_Log_Info_Message()
    {
        Log.Logger.Information("This is a info message from unit test!");

        Assert.Pass();
    }

    [Test]
    public async Task Should_Log_Warning_Message()
    {
        Log.Logger.Warning("This is a warning message from unit test!");

        Assert.Pass();
    }

    [Test]
    public async Task Should_Log_Debug_Message()
    {
        Log.Logger.Debug("This is a debug message from unit test!");

        Assert.Pass();
    }

    [Test]
    public async Task Should_Log_Verbose_Message()
    {
        Log.Logger.Verbose("This is a debug message from unit test!");

        Assert.Pass();
    }

    [Test]
    public async Task Should_Log_Error_Message()
    {
        Log.Logger.Error("This is a error message from unit test!");

        Assert.Pass();
    }

    [Test]
    public async Task Should_Log_Exception()
    {
        try
        {
            var x = 0;

            var y = 1 / x;
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An exception occurred while executing the unit test.");
        }
    }
}
