using System.Diagnostics;
using WorkshopBooking.Application.Outcomes;

namespace WorkshopBooking.Api.Infrastructure;

internal static class ApiProblemFactory
{
    public static ApiProblemDetails FromFailure(HttpContext context, ApplicationFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);

        (int status, string title, string detail) = failure.Kind switch
        {
            ApplicationFailureKind.Validation =>
                (StatusCodes.Status400BadRequest, "Request validation failed", "One or more request fields are invalid."),
            ApplicationFailureKind.NotFound =>
                (StatusCodes.Status404NotFound, "Resource not found", "The requested resource was not found."),
            ApplicationFailureKind.Conflict =>
                (StatusCodes.Status409Conflict, "Request conflict", "The request conflicts with the current resource state."),
            _ => throw new ArgumentOutOfRangeException(nameof(failure), "Unknown application failure kind.")
        };

        var errors = failure.Errors?.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.ToArray(),
            StringComparer.Ordinal);

        return Create(context, status, failure.Code, title, detail, errors);
    }

    public static ApiProblemDetails ForStatusCode(HttpContext context, int status)
    {
        (string code, string title, string detail) = status switch
        {
            StatusCodes.Status400BadRequest =>
                ("validation_failed", "Request validation failed", "The request is invalid."),
            StatusCodes.Status404NotFound =>
                ("route_not_found", "Route not found", "The requested API route was not found."),
            StatusCodes.Status405MethodNotAllowed =>
                ("method_not_allowed", "Method not allowed", "The HTTP method is not supported for this route."),
            StatusCodes.Status415UnsupportedMediaType =>
                ("unsupported_media_type", "Unsupported media type", "The request content type is not supported."),
            _ => ("http_error", "Request failed", "The API request could not be completed.")
        };

        IReadOnlyDictionary<string, string[]>? errors = status == StatusCodes.Status400BadRequest
            ? new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["$"] = ["The request is invalid."]
            }
            : null;

        return Create(context, status, code, title, detail, errors);
    }

    public static ApiProblemDetails InternalError(HttpContext context) => Create(
        context,
        StatusCodes.Status500InternalServerError,
        "internal_error",
        "Internal server error",
        "An unexpected error occurred.",
        null);

    private static ApiProblemDetails Create(
        HttpContext context,
        int status,
        string code,
        string title,
        string detail,
        IReadOnlyDictionary<string, string[]>? errors)
    {
        string traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        return new ApiProblemDetails(
            $"urn:workshop-booking:problem:{code}",
            title,
            status,
            detail,
            context.Request.Path.HasValue ? context.Request.Path.Value! : "/",
            code,
            traceId,
            errors);
    }
}
