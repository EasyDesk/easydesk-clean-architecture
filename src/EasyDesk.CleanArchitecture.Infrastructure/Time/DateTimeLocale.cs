using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Time
{
    public class DateTimeLocale : ILocale
    {
        private readonly TimeZoneInfo _timeZoneInfo;

        public DateTimeLocale(DateTimeLocaleSettings settings)
        {
            _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId);
        }

        public LocalDateTime ToLocal(Timestamp timestamp) =>
            LocalDateTime.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(timestamp.AsDateTime, _timeZoneInfo));

        public Timestamp ToTimestamp(LocalDateTime localDateTime) =>
            Timestamp.FromUtcDateTime(TimeZoneInfo.ConvertTimeToUtc(localDateTime.AsDateTime, _timeZoneInfo));
    }

    public class DateTimeLocaleSettings
    {
        public string TimeZoneId { get; set; }
    }
}
