using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Fermat.Logging.Serilog.Webhook.Extensions;

public static class CustomWebhookSinkExtensions
{
    public static LoggerConfiguration CustomWebhook(
        this LoggerSinkConfiguration loggerConfiguration,
        string webhookUrl,
        string method = "POST",
        Dictionary<string, string>? headers = null,
        int batchSizeLimit = 100,
        TimeSpan? period = null,
        int queueLimit = 1000,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information,
        IFormatProvider? formatProvider = null)
    {
        var sink = new CustomWebhookSink(webhookUrl, method, headers, formatProvider, restrictedToMinimumLevel);

        return loggerConfiguration.Sink(sink, new BatchingOptions
        {
            EagerlyEmitFirstEvent = true,
            BatchSizeLimit = batchSizeLimit,
            BufferingTimeLimit = period ?? TimeSpan.FromSeconds(10),
            QueueLimit = queueLimit
        });
    }
}