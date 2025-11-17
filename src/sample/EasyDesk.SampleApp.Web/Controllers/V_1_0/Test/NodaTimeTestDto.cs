using NodaTime;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;

public record NodaTimeTestDto
{
    public Instant Instant { get; init; } = Instant.FromDateTimeUtc(new(1997, 11, 8, 12, 11, 21, DateTimeKind.Utc));

    public OffsetDateTime OffsetDateTime { get; init; } = OffsetDateTime.FromDateTimeOffset(new(1234, 10, 07, 11, 12, 13, TimeSpan.FromMinutes(17)));

    public ZonedDateTime ZonedDateTime { get; init; } = Instant.FromDateTimeUtc(new(2001, 9, 9, 1, 2, 3, DateTimeKind.Utc)).InZone(DateTimeZoneProviders.Tzdb["America/New_York"]);

    public LocalDateTime LocalDateTime { get; init; } = new(2020, 5, 15, 14, 15, 16);

    public LocalDate LocalDate { get; init; } = new(2022, 3, 4);

    public LocalTime LocalTime { get; init; } = new(18, 19, 20);

    public Offset Offset { get; init; } = Offset.FromHoursAndMinutes(-5, 30);

    public Interval Interval { get; init; } = new(
        Instant.FromDateTimeUtc(new(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
        Instant.FromDateTimeUtc(new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

    public Duration Duration { get; init; } = Duration.FromHours(36);

    public Period Period { get; init; } = Period.FromDays(10);
}
