namespace WorkshopBooking.Api.Infrastructure;

internal sealed partial class ApiExceptionMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (IsApiRequest(context) && !context.Response.HasStarted)
        {
            LogUnhandledError(logger, exception, context.Request.Method, context.Request.Path);

            context.Response.Clear();
            await ApiProblemResponseWriter.WriteAsync(
                context,
                ApiProblemFactory.InternalError(context));
        }
    }

    internal static bool IsApiRequest(HttpContext context) =>
        context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Unhandled error while processing {Method} {Path}")]
    private static partial void LogUnhandledError(
        ILogger logger,
        Exception exception,
        string method,
        PathString path);
}
