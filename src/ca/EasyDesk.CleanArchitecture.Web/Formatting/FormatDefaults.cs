using NodaTime.Text;

namespace EasyDesk.CleanArchitecture.Web.Formatting;

public static class FormatDefaults
{
    public static readonly AnnualDatePattern AnnualDate = AnnualDatePattern.Iso;
    public static readonly DurationPattern Duration = DurationPattern.Roundtrip;
    public static readonly InstantPattern Instant = InstantPattern.ExtendedIso;
    public static readonly LocalDateTimePattern LocalDateTime = LocalDateTimePattern.ExtendedIso;
    public static readonly LocalDatePattern LocalDate = LocalDatePattern.Iso;
    public static readonly LocalTimePattern LocalTime = LocalTimePattern.ExtendedIso;
    public static readonly OffsetPattern Offset = OffsetPattern.GeneralInvariantWithZ;
    public static readonly OffsetDateTimePattern OffsetDateTime = OffsetDateTimePattern.ExtendedIso;
    public static readonly OffsetDatePattern OffsetDate = OffsetDatePattern.GeneralIso;
    public static readonly OffsetTimePattern OffsetTime = OffsetTimePattern.ExtendedIso;
    public static readonly PeriodPattern Period = PeriodPattern.Roundtrip;
    public static readonly YearMonthPattern YearMonth = YearMonthPattern.Iso;
    public static readonly ZonedDateTimePattern ZonedDateTime = ZonedDateTimePattern.ExtendedFormatOnlyIso;
}
