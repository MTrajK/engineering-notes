using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace WorkshopBooking.Api.Infrastructure;

internal sealed class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    private static readonly Regex _explicitOffsetTimestamp = new(
        @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,7})?(?:Z|[+-]\d{2}:\d{2})$",
        RegexOptions.CultureInvariant);

    public override DateTimeOffset Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("A timestamp must be an ISO 8601 string with an explicit offset.");
        }

        string? value = reader.GetString();

        if (value is null
            || !_explicitOffsetTimestamp.IsMatch(value)
            || !DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset parsed))
        {
            throw new JsonException("A timestamp must include Z or a numeric UTC offset.");
        }

        return parsed;
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset value,
        JsonSerializerOptions options)
    {
        string utcTimestamp = value.UtcDateTime.ToString(
            "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
            CultureInfo.InvariantCulture);
        writer.WriteStringValue(utcTimestamp);
    }
}
