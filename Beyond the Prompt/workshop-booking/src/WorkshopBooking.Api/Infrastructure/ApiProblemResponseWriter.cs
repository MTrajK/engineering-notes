using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace WorkshopBooking.Api.Infrastructure;

internal static class ApiProblemResponseWriter
{
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public static async Task WriteAsync(HttpContext context, ApiProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status;
        context.Response.ContentType = "application/problem+json";

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problem,
            _serializerOptions,
            context.RequestAborted);
    }

    public static ObjectResult ToObjectResult(ApiProblemDetails problem)
    {
        ObjectResult result = new(problem)
        {
            StatusCode = problem.Status
        };
        result.ContentTypes.Add("application/problem+json");
        return result;
    }
}
