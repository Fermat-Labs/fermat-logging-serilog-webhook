using Serilog.Events;

namespace Fermat.Logging.Serilog.Webhook.Models;

public class WebhookOptions
{
    public bool Enabled { get; set; } = true;
    public string? Url { get; set; }
    public string Method { get; set; } = "POST";
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>()
    {
        { "Content-Type", "application/json" }
    };
    public int BatchSizeLimit { get; set; } = 50;
    public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(5);
    public int QueueLimit { get; set; } = 500;
    public LogEventLevel RestrictedToMinimumLevel { get; set; } = LogEventLevel.Error;
}