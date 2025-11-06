using Fermat.Logging.Serilog.Models;
using Fermat.Logging.Serilog.Webhook.Models;

namespace Fermat.Logging.Serilog.Webhook.Extensions;

/// <summary>
/// Extension methods for configuring Webhook options in SerilogOptions.
/// </summary>
public static class SerilogOptionExtensions
{
    private const string WebhookOptionsKey = "Fermat.Logging.Serilog.Webhook.Options";

    /// <summary>
    /// Gets or creates WebhookOptions from SerilogOptions.
    /// </summary>
    public static WebhookOptions GetWebhookOptions(this SerilogOptions options)
    {
        var webhookOptions = options.GetExtensionOption<WebhookOptions>(WebhookOptionsKey);
        if (webhookOptions == null)
        {
            webhookOptions = new WebhookOptions();
            options.SetExtensionOption(WebhookOptionsKey, webhookOptions);
        }
        return webhookOptions;
    }

    /// <summary>
    /// Sets WebhookOptions in SerilogOptions.
    /// </summary>
    public static void SetWebhookOptions(this SerilogOptions options, WebhookOptions webhookOptions)
    {
        options.SetExtensionOption(WebhookOptionsKey, webhookOptions);
    }
}