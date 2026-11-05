using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WorkshopBooking.Application.Outcomes;

namespace WorkshopBooking.Api.Infrastructure;

internal static class ModelStateProblemFactory
{
    public static IActionResult Create(ActionContext context)
    {
        Dictionary<string, List<string>> collectedErrors = new(StringComparer.Ordinal);

        foreach ((string key, ModelStateEntry? entry) in context.ModelState)
        {
            if (entry is null)
            {
                continue;
            }

            foreach (ModelError error in entry.Errors)
            {
                bool isJsonError = ContainsJsonException(error.Exception)
                    || key.StartsWith('$');
                string field = isJsonError ? "$" : ToPublicFieldName(key);
                string message = error.Exception is not null
                    ? "The supplied value is invalid."
                    : string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The request is invalid."
                        : error.ErrorMessage;

                if (!collectedErrors.TryGetValue(field, out List<string>? messages))
                {
                    messages = [];
                    collectedErrors.Add(field, messages);
                }

                messages.Add(message);
            }
        }

        if (collectedErrors.Count == 0)
        {
            collectedErrors.Add("$", ["The request is invalid."]);
        }

        var validationErrors = collectedErrors.ToDictionary(
            entry => entry.Key,
            entry => (IReadOnlyList<string>)entry.Value,
            StringComparer.Ordinal);
        var failure = ApplicationFailure.Validation(validationErrors);
        ApiProblemDetails problem = ApiProblemFactory.FromFailure(context.HttpContext, failure);
        return ApiProblemResponseWriter.ToObjectResult(problem);
    }

    private static bool ContainsJsonException(Exception? exception)
    {
        while (exception is not null)
        {
            if (exception is JsonException)
            {
                return true;
            }

            exception = exception.InnerException;
        }

        return false;
    }

    private static string ToPublicFieldName(string memberPath)
    {
        if (string.IsNullOrWhiteSpace(memberPath) || memberPath == "$")
        {
            return "$";
        }

        string path = memberPath.StartsWith("$.", StringComparison.Ordinal)
            ? memberPath[2..]
            : memberPath.StartsWith('$')
                ? memberPath[1..].TrimStart('.')
                : memberPath;

        if (path.Length == 0)
        {
            return "$";
        }

        string[] segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);

        for (int index = 0; index < segments.Length; index++)
        {
            string segment = segments[index];
            segments[index] = char.ToLowerInvariant(segment[0]) + segment[1..];
        }

        return segments.Length == 0 ? "$" : string.Join('.', segments);
    }
}
