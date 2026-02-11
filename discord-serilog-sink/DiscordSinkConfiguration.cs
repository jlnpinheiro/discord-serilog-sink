using JNogueira.Discord.WebhookClient;
using Microsoft.AspNetCore.Http;
using Serilog.Events;

namespace JNogueira.Discord.Serilog;

public sealed class DiscordSinkConfiguration
{
    public string ApplicationName { get; set; }
    public string MessageUserName { get; set; }
    public string EnvironmentName { get; set; }
    public IHttpContextAccessor HttpContextAccessor { get; set; }
    public IEnumerable<UserClaimValueToDiscordField> UserClaimValueToDiscordFields { get; set; } = [];
    public IEnumerable<DiscordMessageEmbedField> MessageEmbedFields { get; set; } = [];
    public LogEventLevel MinLogEventLevel { get; set; } = LogEventLevel.Debug;
}

public record UserClaimValueToDiscordField
{
    public string ClaimType { get; init; }
    public string DiscordFieldName { get; init; }

    public UserClaimValueToDiscordField(string claimType, string discordFieldName)
    {
        ClaimType = claimType?.Trim();
        DiscordFieldName = discordFieldName?.Trim();
    }
}