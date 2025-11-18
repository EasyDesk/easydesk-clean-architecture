using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Web.OpenApi.NodaTime;

public static class SwaggerGenOptionsExtensions
{
    public static void ConfigureForNodaTimeWithSystemTextJson(
        this SwaggerGenOptions config,
        JsonSerializerOptions? jsonSerializerOptions = null,
        Action<JsonSerializerOptions>? configureSerializerOptions = null,
        IDateTimeZoneProvider? dateTimeZoneProvider = null,
        bool shouldGenerateExamples = true,
        NodaTimeSchemaExamples? schemaExamples = null)
    {
        jsonSerializerOptions ??= new();
        configureSerializerOptions?.Invoke(jsonSerializerOptions);

        jsonSerializerOptions.ConfigureForNodaTime(dateTimeZoneProvider ?? DateTimeZoneProviders.Tzdb);

        var nodaTimeSchemaSettings = new NodaTimeSchemaSettings(
            jsonSerializerOptions,
            shouldGenerateExamples: shouldGenerateExamples,
            schemaExamples,
            dateTimeZoneProvider);

        var schemas = new NodaTimeSchemasFactory(nodaTimeSchemaSettings).CreateSchemas();

        config.MapType<Instant>(schemas.Instant);
        config.MapType<LocalDate>(schemas.LocalDate);
        config.MapType<LocalTime>(schemas.LocalTime);
        config.MapType<LocalDateTime>(schemas.LocalDateTime);
        config.MapType<OffsetDateTime>(schemas.OffsetDateTime);
        config.MapType<ZonedDateTime>(schemas.ZonedDateTime);
        config.MapType<Interval>(schemas.Interval);
        config.MapType<DateInterval>(schemas.DateInterval);
        config.MapType<Offset>(schemas.Offset);
        config.MapType<Period>(schemas.Period);
        config.MapType<Duration>(schemas.Duration);
        config.MapType<OffsetDate>(schemas.OffsetDate);
        config.MapType<OffsetTime>(schemas.OffsetTime);
        config.MapType<DateTimeZone>(schemas.DateTimeZone);

        // Nullable structs
        config.MapType<Instant?>(schemas.Instant);
        config.MapType<LocalDate?>(schemas.LocalDate);
        config.MapType<LocalTime?>(schemas.LocalTime);
        config.MapType<LocalDateTime?>(schemas.LocalDateTime);
        config.MapType<OffsetDateTime?>(schemas.OffsetDateTime);
        config.MapType<ZonedDateTime?>(schemas.ZonedDateTime);
        config.MapType<Interval?>(schemas.Interval);
        config.MapType<Offset?>(schemas.Offset);
        config.MapType<Duration?>(schemas.Duration);
        config.MapType<OffsetDate?>(schemas.OffsetDate);
        config.MapType<OffsetTime?>(schemas.OffsetTime);
    }
}
