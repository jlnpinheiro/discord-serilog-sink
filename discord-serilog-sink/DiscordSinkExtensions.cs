using JNogueira.Discord.WebhookClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Configuration;

namespace JNogueira.Discord.Serilog;

public static class DiscordSinkExtensions
{
    public static IServiceCollection AddSerilogWithDiscordSink(this IServiceCollection services, string discordWebhookUrl)
    {
        services.AddHttpContextAccessor();

        services.AddDiscordWebhookClient(discordWebhookUrl);

        return services;
    }

    public static IServiceCollection AddSerilogWithDiscordSink(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();

        services.AddDiscordWebhookClient(config["Serilog:Discord:WebhookUrl"]);

        services.Configure<DiscordSinkConfiguration>(config.GetSection("Serilog:Discord"));

        return services;
    }

    public static LoggerConfiguration WriteToDiscord(this LoggerConfiguration loggerConfig, IServiceProvider services, IConfiguration config)
    {
        var webhookClient = services.GetRequiredService<DiscordWebhookClient>();

        var sinkConfig = services.GetRequiredService<IOptions<DiscordSinkConfiguration>>().Value;

        if (string.IsNullOrEmpty(sinkConfig.EnvironmentName))
            sinkConfig.EnvironmentName = config["DOTNET_ENVIRONMENT"] ?? config["ASPNETCORE_ENVIRONMENT"];

        sinkConfig.HttpContextAccessor = services.GetService<IHttpContextAccessor>();

        return loggerConfig
            .MinimumLevel.Is(sinkConfig.MinLogEventLevel)
            .WriteTo.Discord(webhookClient, sinkConfig, null);
    }

    public static LoggerConfiguration WriteToDiscord(this LoggerConfiguration loggerConfig, IServiceProvider services, DiscordSinkConfiguration sinkConfig)
    {
        var webhookClient = services.GetRequiredService<DiscordWebhookClient>();

        return loggerConfig
            .MinimumLevel.Is(sinkConfig.MinLogEventLevel)
            .WriteTo.Discord(webhookClient, sinkConfig, null);
    }

    private static LoggerConfiguration Discord(
        this LoggerSinkConfiguration loggerConfiguration,
        DiscordWebhookClient webHookClient,
        DiscordSinkConfiguration config,
        IFormatProvider formatProvider = null)
    {
        return loggerConfiguration.Sink(
            new DiscordSink(webHookClient, config, formatProvider));
    }
}
