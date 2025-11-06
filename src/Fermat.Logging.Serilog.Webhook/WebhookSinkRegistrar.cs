using Fermat.Logging.Serilog.Interfaces;
using Fermat.Logging.Serilog.Models;
using Fermat.Logging.Serilog.Webhook.Extensions;
using Serilog;

namespace Fermat.Logging.Serilog.Webhook;

public class WebhookSinkRegistrar : ILogSinkRegistrar
{
    public void Register(LoggerConfiguration configuration, SerilogOptions options)
    {
        var webhookOptions = options.GetWebhookOptions();
        if (!webhookOptions.Enabled || string.IsNullOrEmpty(webhookOptions.Url))
            return;

        configuration.WriteTo.CustomWebhook(
            webhookUrl: webhookOptions.Url!,
            method: webhookOptions.Method,
            headers: webhookOptions.Headers,
            batchSizeLimit: webhookOptions.BatchSizeLimit,
            period: webhookOptions.Period,
            queueLimit: webhookOptions.QueueLimit,
            restrictedToMinimumLevel: webhookOptions.RestrictedToMinimumLevel
        );
    }
}