using NodaTime;

namespace EasyDesk.CleanArchitecture.Web.OpenApi.NodaTime;

public record NodaTimeSchemaExamples
{
    public DateTimeZone DateTimeZone { get; set; }

    public Instant Instant { get; set; }

    public ZonedDateTime ZonedDateTime { get; set; }

    public Interval Interval { get; set; }

    public DateInterval DateInterval { get; set; }

    public Period Period { get; set; }

    public OffsetDate OffsetDate { get; set; }

    public OffsetTime OffsetTime { get; set; }

    public OffsetDateTime OffsetDateTime { get; set; }

    public NodaTimeSchemaExamples(
        IDateTimeZoneProvider dateTimeZoneProvider,
        DateTime? dateTimeUtc = null,
        string? dateTimeZone = null)
    {
        var dateTimeUtcValue = dateTimeUtc ?? DateTime.UnixEpoch;
        if (dateTimeUtcValue.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("dateTimeUtc should be UTC", nameof(dateTimeUtc));
        }

        DateTimeZone = dateTimeZone is not null
            ? dateTimeZoneProvider.GetZoneOrNull(dateTimeZone) ?? dateTimeZoneProvider.GetSystemDefault()
            : dateTimeZoneProvider.GetSystemDefault();

        Instant = Instant.FromDateTimeUtc(dateTimeUtcValue);

        ZonedDateTime = Instant.InZone(DateTimeZone);

        Interval = new(Instant,
            Instant.PlusTicks(TimeSpan.TicksPerDay)
                .PlusTicks(TimeSpan.TicksPerHour)
                .PlusTicks(TimeSpan.TicksPerMinute)
                .PlusTicks(TimeSpan.TicksPerSecond)
                .PlusTicks(TimeSpan.TicksPerMillisecond));

        DateInterval = new(ZonedDateTime.Date, ZonedDateTime.Date.PlusDays(1));

        Period = Period.Between(ZonedDateTime.LocalDateTime, Interval.End.InZone(DateTimeZone).LocalDateTime, PeriodUnits.AllUnits);

        OffsetDate = new(ZonedDateTime.Date, ZonedDateTime.Offset);

        OffsetTime = new(ZonedDateTime.TimeOfDay, ZonedDateTime.Offset);

        OffsetDateTime = Instant.WithOffset(ZonedDateTime.Offset);
    }
}
