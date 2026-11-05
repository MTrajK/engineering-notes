using System.Text.Json.Serialization;

namespace WorkshopBooking.Api.Infrastructure;

public sealed record ApiProblemDetails(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("detail")] string Detail,
    [property: JsonPropertyName("instance")] string Instance,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("traceId")] string TraceId,
    [property: JsonPropertyName("errors")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyDictionary<string, string[]>? Errors);
