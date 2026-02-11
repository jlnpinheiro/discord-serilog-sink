using JNogueira.Discord.WebhookClient;
using Serilog.Core;
using Serilog.Events;
using System.Collections;
using System.Text;
using System.Text.Json;

namespace JNogueira.Discord.Serilog;

public class DiscordSink(DiscordWebhookClient webHookClient, DiscordSinkConfiguration config, IFormatProvider formatProvider) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        SendWebhookMessage(logEvent);
    }

    private void SendWebhookMessage(LogEvent logEvent)
    {
        var renderMessage = logEvent.RenderMessage(formatProvider);

        var messageContent = string.Empty;

        int? messageEmbedColor = null;

        switch(logEvent.Level)
        {
            case LogEventLevel.Verbose:
            case LogEventLevel.Debug:
                messageContent = $"{DiscordEmoji.SpiderWeb} **{logEvent.Level}**: {renderMessage}";
                break;
            case LogEventLevel.Information:
                messageContent = $"{DiscordEmoji.InformationSource} **{logEvent.Level}**: {renderMessage}";
                messageEmbedColor = (int)DiscordColor.Blue;
                break;
            case LogEventLevel.Warning:
                messageContent = $"{DiscordEmoji.Warning} **{logEvent.Level}**: {renderMessage}";
                messageEmbedColor = (int)DiscordColor.Yellow;
                break;
            case LogEventLevel.Error:
                messageContent = $"{DiscordEmoji.Skull} **{logEvent.Level}**: {renderMessage}";
                messageEmbedColor = (int)DiscordColor.Red;
                break;
            case LogEventLevel.Fatal:
                messageContent = $"{DiscordEmoji.Radioactive} **{logEvent.Level}**: {renderMessage}";
                messageEmbedColor = 16711680;
                break;
        }

        var fields = new List<DiscordMessageEmbedField>();

        foreach(var item in config.MessageEmbedFields.Where(x => !string.IsNullOrEmpty(x.Name)))
            fields.Add(new DiscordMessageEmbedField(item.Name, item.Value));

        if (!string.IsNullOrEmpty(config.ApplicationName))
            fields.Add(new DiscordMessageEmbedField("Application name", config.ApplicationName));

        if (!string.IsNullOrEmpty(config.EnvironmentName))
            fields.Add(new DiscordMessageEmbedField("Environment name", config.EnvironmentName));

        if (config.HttpContextAccessor?.HttpContext?.User is not null && config.UserClaimValueToDiscordFields.Any())
        {
            foreach (var item in config.UserClaimValueToDiscordFields.Where(x => !string.IsNullOrEmpty(x.DiscordFieldName)))
            {
                var claim = config.HttpContextAccessor.HttpContext.User?.Claims?.FirstOrDefault(x => x.Type.Equals(item.ClaimType, StringComparison.CurrentCultureIgnoreCase)
                    || x.Type.EndsWith(item.ClaimType, StringComparison.CurrentCultureIgnoreCase));

                if (claim is null)
                    fields.Add(new DiscordMessageEmbedField(item.DiscordFieldName, $"Claim {item.ClaimType} not found"));
                else
                    fields.Add(new DiscordMessageEmbedField(item.DiscordFieldName, claim.Value ?? "null"));
            }
        }

        var files = new List<DiscordFile>();

        DiscordMessageEmbed embed = null;

        if (logEvent.Exception is null)
        {
            if (fields.Count > 0)
            {
                embed = new DiscordMessageEmbed(color: messageEmbedColor, fields: [.. fields]);
            }
        }
        else
        {
            fields.Add(new DiscordMessageEmbedField("Exception type", logEvent.Exception.GetType().ToString()));

            if (logEvent.Exception.Source is not null)
            {
                fields.Add(new DiscordMessageEmbedField("Source", logEvent.Exception.Source));
            }

            var exceptionInfoText = new StringBuilder();

            exceptionInfoText.Append("Message: ").AppendLine(logEvent.Exception.Message);
            exceptionInfoText.Append("Exception type: ").AppendLine(logEvent.Exception.GetType().ToString());
            exceptionInfoText.Append("Source: ").AppendLine(logEvent.Exception.Source);
            exceptionInfoText.Append("Base exception: ").AppendLine(logEvent.Exception.GetBaseException()?.Message);

            foreach (DictionaryEntry data in logEvent.Exception.Data)
                exceptionInfoText.Append(data.Key).Append(": ").Append(data.Value).AppendLine();

            exceptionInfoText.Append("Stack trace: ").AppendLine(logEvent.Exception.StackTrace);

            if (config.HttpContextAccessor?.HttpContext?.Request != null)
            {
                var uriBuilder = new UriBuilder
                {
                    Scheme = config.HttpContextAccessor.HttpContext.Request.Scheme,
                    Host = config.HttpContextAccessor.HttpContext.Request.Host.Host,
                    Path = config.HttpContextAccessor.HttpContext.Request.Path.ToString(),
                    Query = config.HttpContextAccessor.HttpContext.Request.QueryString.ToString()
                };

                if (config.HttpContextAccessor.HttpContext.Request.Host.Port.HasValue && config.HttpContextAccessor.HttpContext.Request.Host.Port.Value != 80)
                    uriBuilder.Port = config.HttpContextAccessor.HttpContext.Request.Host.Port.Value;

                if (!string.IsNullOrEmpty(uriBuilder.Host))
                    fields.Add(new DiscordMessageEmbedField("URL", uriBuilder.Uri.ToString()));

                var requestHeaders = new Dictionary<string, string>();

                foreach (var item in config.HttpContextAccessor.HttpContext.Request.Headers?.Where(x => x.Key != "Cookie" && x.Value.Count > 0))
                    requestHeaders.Add(item.Key, string.Join(",", [.. item.Value]));

                if (requestHeaders.Count > 0)
                {
                    exceptionInfoText
                        .AppendLine("Request headers:")
                        .AppendLine(JsonSerializer.Serialize(requestHeaders, _defaultJsonSerializerOptions));
                }
            }

            files.Add(new DiscordFile("exception-details.txt", Encoding.UTF8.GetBytes(exceptionInfoText.ToString())));

            embed = new DiscordMessageEmbed(color: messageEmbedColor, description: $"**{logEvent.Exception.Message}**", fields: fields.Count > 0 ? fields.ToArray() : null);
        }

        var message = new DiscordMessage(messageContent, config.MessageUserName, embeds: embed != null ? [embed] : null);

        if (files.Count > 0)
        {
            _ = webHookClient.SendToDiscordAsync(message, [.. files], true).GetAwaiter().GetResult();
        }
        else
        {
            _ = webHookClient.SendToDiscordAsync(message, sendMessageAsFileAttachmentOnError: true).GetAwaiter().GetResult();
        }
    }

    private static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new() { WriteIndented = true };
}
