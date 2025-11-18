using Microsoft.OpenApi;
using NodaTime;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Web.OpenApi.NodaTime;

public class NodaTimeSchemasFactory
{
    private readonly NodaTimeSchemaSettings _settings;

    public NodaTimeSchemasFactory(NodaTimeSchemaSettings settings)
    {
        _settings = settings;
    }

    public NodaTimeSchemas CreateSchemas()
    {
        var examples = _settings.SchemaExamples;

        return new()
        {
            Instant = () => StringSchema(examples.Instant, "date-time"),
            LocalDate = () => StringSchema(examples.ZonedDateTime.Date, "date"),
            LocalTime = () => StringSchema(examples.ZonedDateTime.TimeOfDay),
            LocalDateTime = () => StringSchema(examples.ZonedDateTime.LocalDateTime),
            OffsetDateTime = () => StringSchema(examples.OffsetDateTime, "date-time"),
            ZonedDateTime = () => StringSchema(examples.ZonedDateTime),
            Interval = () => new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    { ResolvePropertyName(nameof(Interval.Start)), StringSchema(examples.Interval.Start, "date-time") },
                    { ResolvePropertyName(nameof(Interval.End)), StringSchema(examples.Interval.End, "date-time") },
                },
            },
            DateInterval = () => new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    { ResolvePropertyName(nameof(DateInterval.Start)), StringSchema(examples.DateInterval.Start, "date") },
                    { ResolvePropertyName(nameof(DateInterval.End)), StringSchema(examples.DateInterval.End, "date") },
                },
            },
            Offset = () => StringSchema(examples.ZonedDateTime.Offset),
            Period = () => StringSchema(examples.Period),
            Duration = () => StringSchema(examples.Interval.Duration),
            OffsetDate = () => StringSchema(examples.OffsetDate),
            OffsetTime = () => StringSchema(examples.OffsetTime),
            DateTimeZone = () => StringSchema(examples.DateTimeZone),
        };
    }

    private OpenApiSchema StringSchema(object exampleObject, string? format = null)
    {
        return new()
        {
            Type = JsonSchemaType.String,
            Example = _settings.ShouldGenerateExamples
                ? FormatToJson(exampleObject)
                : null,
            Format = format,
        };
    }

    private string FormatToJson(object value)
    {
        var formatToJson = JsonSerializer.Serialize(value, _settings.SerializerOptions);
        if (formatToJson.StartsWith('\"') && formatToJson.EndsWith('\"'))
        {
            formatToJson = formatToJson[1..^1];
        }

        return formatToJson;
    }

    private string ResolvePropertyName(string clrPropertyName)
    {
        var jsonPropertyName = _settings.SerializerOptions
            .PropertyNamingPolicy
            ?.ConvertName(clrPropertyName)
            ?? clrPropertyName;
        return jsonPropertyName;
    }
}
