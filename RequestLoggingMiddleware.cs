using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Generate a short 8-character correlation ID from a GUID
        string correlationId = System.Guid.NewGuid().ToString("N")[..8];

        // 2. Stamp the ID into the response headers BEFORE downstream execution
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        // 3. Start timing the request
        var stopwatch = Stopwatch.StartNew();

        // 4. Log Entry Line: HTTP Method, Path, and Correlation ID
        _logger.LogInformation("HTTP {Method} {Path} started. [Correlation ID: {CorrelationId}]", 
            context.Request.Method, 
            context.Request.Path, 
            correlationId);

        // 5. Pass control to the next middleware in the pipeline
        await _next(context);

        // 6. Stop timing
        stopwatch.Stop();

        // 7. Log Exit Line: Status Code, Elapsed Milliseconds, and Correlation ID
        _logger.LogInformation("HTTP request finished with Status Code {StatusCode} in {ElapsedMs}ms. [Correlation ID: {CorrelationId}]", 
            context.Response.StatusCode, 
            stopwatch.ElapsedMilliseconds, 
            correlationId);
    }
}