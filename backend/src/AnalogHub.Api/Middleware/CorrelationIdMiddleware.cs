using Serilog.Context;

namespace AnalogHub.Api.Middleware;

/// <summary>
/// Reads (or generates) an X-Correlation-Id per request and pushes it into Serilog's LogContext, so
/// every log line written anywhere during that request — including deep inside a MediatR handler —
/// carries the same ID. Echoed back on the response so a client can correlate its own logs too.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing) && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : context.TraceIdentifier;

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
