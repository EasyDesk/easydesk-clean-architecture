using EasyDesk.CleanArchitecture.Web.OpenApi.NodaTime;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using NodaTime;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

public static partial class OpenApiUtils
{
    public static void ConfigureForNodaTimeWithSystemTextJson(
        this OpenApiOptions options,
        JsonSerializerOptions jsonSerializerOptions,
        IDateTimeZoneProvider? dateTimeZoneProvider = null,
        bool shouldGenerateExamples = true,
        NodaTimeSchemaExamples? schemaExamples = null)
    {
        var nodaTimeSchemaSettings = new NodaTimeSchemaSettings(
            jsonSerializerOptions,
            shouldGenerateExamples: shouldGenerateExamples,
            schemaExamples,
            dateTimeZoneProvider);

        var schemas = new NodaTimeSchemasFactory(nodaTimeSchemaSettings).CreateSchemas();

        options.MapType<Instant>(schemas.Instant);
        options.MapType<LocalDate>(schemas.LocalDate);
        options.MapType<LocalTime>(schemas.LocalTime);
        options.MapType<LocalDateTime>(schemas.LocalDateTime);
        options.MapType<OffsetDateTime>(schemas.OffsetDateTime);
        options.MapType<ZonedDateTime>(schemas.ZonedDateTime);
        options.MapType<Interval>(schemas.Interval);
        options.MapType<DateInterval>(schemas.DateInterval);
        options.MapType<Offset>(schemas.Offset);
        options.MapType<Period>(schemas.Period);
        options.MapType<Duration>(schemas.Duration);
        options.MapType<OffsetDate>(schemas.OffsetDate);
        options.MapType<OffsetTime>(schemas.OffsetTime);
        options.MapType<DateTimeZone>(schemas.DateTimeZone);

        // Nullable structs
        options.MapType<Instant?>(schemas.Instant);
        options.MapType<LocalDate?>(schemas.LocalDate);
        options.MapType<LocalTime?>(schemas.LocalTime);
        options.MapType<LocalDateTime?>(schemas.LocalDateTime);
        options.MapType<OffsetDateTime?>(schemas.OffsetDateTime);
        options.MapType<ZonedDateTime?>(schemas.ZonedDateTime);
        options.MapType<Interval?>(schemas.Interval);
        options.MapType<Offset?>(schemas.Offset);
        options.MapType<Duration?>(schemas.Duration);
        options.MapType<OffsetDate?>(schemas.OffsetDate);
        options.MapType<OffsetTime?>(schemas.OffsetTime);
    }

    public static void MapType<T>(this OpenApiOptions options, Func<OpenApiSchema> schema)
    {
        options.AddSchemaTransformer((s, context, _) =>
        {
            if (context.JsonTypeInfo.Type == typeof(T))
            {
                s.CopyFunctionalFieldsFrom(schema());
            }
            return Task.CompletedTask;
        });
    }
}
