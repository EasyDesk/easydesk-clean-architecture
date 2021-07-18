using EasyDesk.Tools.PrimitiveTypes.DateAndTime;

namespace EasyDesk.CleanArchitecture.Domain.Time
{
    public interface ILocale
    {
        LocalDateTime ToLocal(Timestamp timestamp);

        Timestamp ToTimestamp(LocalDateTime localDateTime);
    }

    public static class LocalTimeConversions
    {
        public static Date LocalToday(this ILocale locale, ITimestampProvider timestampProvider) =>
            locale.ToLocal(timestampProvider.Now).Date;
    }
}
