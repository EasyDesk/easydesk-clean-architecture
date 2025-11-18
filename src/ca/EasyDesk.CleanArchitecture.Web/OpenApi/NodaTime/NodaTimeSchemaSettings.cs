using NodaTime;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Web.OpenApi.NodaTime;

public record NodaTimeSchemaSettings
{
    public JsonSerializerOptions SerializerOptions { get; }

    public IDateTimeZoneProvider DateTimeZoneProvider { get; }

    public bool ShouldGenerateExamples { get; }

    public NodaTimeSchemaExamples SchemaExamples { get; }

    public NodaTimeSchemaSettings(
        JsonSerializerOptions serializerOptions,
        bool shouldGenerateExamples,
        NodaTimeSchemaExamples? schemaExamples = null,
        IDateTimeZoneProvider? dateTimeZoneProvider = null)
    {
        SerializerOptions = serializerOptions;
        DateTimeZoneProvider = dateTimeZoneProvider ?? DateTimeZoneProviders.Tzdb;

        ShouldGenerateExamples = shouldGenerateExamples;
        SchemaExamples = schemaExamples
            ?? new(
                DateTimeZoneProvider,
                dateTimeUtc: null,
                dateTimeZone: null);
    }
}
