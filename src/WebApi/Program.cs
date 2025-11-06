using System.Text.Json;
using System.Text.Json.Serialization;
using Fermat.Logging.Serilog.DependencyInjections;
using Fermat.Logging.Serilog.Webhook.Extensions;
using Fermat.Logging.Serilog.Webhook.Models;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.AddFermatSerilogServices(options =>
{
    options.Console.Enabled = true;
    options.File.Enabled = false;
    options.SetWebhookOptions(new WebhookOptions
    {
        Enabled = true,
        Url = "https://webhook.site/8fe3a2e8-4dd0-4bbe-bffc-1306107fe4b8",
        BatchSizeLimit = 1,
        QueueLimit = 5,
        RestrictedToMinimumLevel = LogEventLevel.Verbose
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();