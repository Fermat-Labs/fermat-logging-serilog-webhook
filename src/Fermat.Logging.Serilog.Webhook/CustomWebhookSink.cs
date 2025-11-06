using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Fermat.Exceptions.Core.Abstractions;
using Fermat.Extensions.Core;
using Serilog.Core;
using Serilog.Events;

namespace Fermat.Logging.Serilog.Webhook;

public class CustomWebhookSink : IBatchedLogEventSink
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;
    private readonly IFormatProvider? _formatProvider;
    private readonly string _method;
    private readonly Dictionary<string, string> _headers;
    private readonly LogEventLevel _restrictedToMinimumLevel;

    public CustomWebhookSink(
        string webhookUrl,
        string method = "POST",
        Dictionary<string, string>? headers = null,
        IFormatProvider? formatProvider = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information)
    {
        _webhookUrl = webhookUrl;
        _method = method;
        _formatProvider = formatProvider;
        _headers = headers ?? new Dictionary<string, string>();
        _restrictedToMinimumLevel = restrictedToMinimumLevel;

        _httpClient = new HttpClient();

        // Default headers
        _headers.TryAdd("Content-Type", "application/json");

        // Custom headers
        foreach (var header in _headers)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    public async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> batch)
    {
        if (batch.Count == 0) return;
        
        Console.WriteLine($"{nameof(CustomWebhookSink)}.{nameof(EmitBatchAsync)}");

        try
        {
            var payload = CreatePayload(batch);
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod(_method), _webhookUrl)
            {
                Content = content
            };
        
            if (_headers.TryGetValue("Content-Type", out var header))
            {
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(header);
            }
        
            var response = await _httpClient.SendAsync(request);
        
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Webhook sink error: HTTP {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CustomWebhookSink: {ex.Message}");
        }
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    private object CreatePayload(IEnumerable<LogEvent> logEvents)
    {
        var logs = logEvents.Where(item => item.Level >= _restrictedToMinimumLevel).Select(logEvent => new
        {
            timestamp = logEvent.Timestamp,
            level = logEvent.Level.ToString(),
            message = logEvent.RenderMessage(_formatProvider),
            exception = logEvent.Exception?.ToString(),
            exceptionProperties = GetExceptionProperties(logEvent.Exception),
            exceptionType = logEvent.Exception?.GetExceptionType(),
            sourceContext = GetSourceContext(logEvent),
            properties = logEvent.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()),
        }).ToList();

        return new
        {
            batchId = Guid.NewGuid(),
            timestamp = DateTime.UtcNow,
            batchSize = logs.Count,
            logs
        };
    }

    private string? GetSourceContext(LogEvent logEvent)
    {
        return logEvent.Properties.TryGetValue("SourceContext", out var sourceContext)
            ? sourceContext.ToString().Trim('"')
            : null;
    }
    
    private Dictionary<string, object?> GetExceptionProperties(Exception? exception)
    {
        var exceptionProperties = new Dictionary<string, object?>();
        if (exception == null) return exceptionProperties;
        
        if (exception is AppException appException)
        {
            exceptionProperties["Code"] = appException.ErrorCode;
            exceptionProperties["StatusCode"] = appException.StatusCode.ToString();
            exceptionProperties["Details"] = appException.Details;

            if (!string.IsNullOrWhiteSpace(appException.CorrelationId))
            {
                exceptionProperties["CorrelationId"] = appException.CorrelationId;
            }
        }

        exceptionProperties["Fingerprint"] = exception.GenerateFingerprint();
        exceptionProperties["ExceptionType"] = exception.GetExceptionType();
        exceptionProperties["ExceptionMessage"] = exception.Message;
        exceptionProperties["Data"] = exception.ConvertExceptionDataToDictionary();
        exceptionProperties["StackTrace"] = exception.StackTrace;
        if (exception.InnerException != null)
        {
            exceptionProperties["InnerException"] = GetExceptionProperties(exception.InnerException);
        }
        
        return exceptionProperties;
    }
}