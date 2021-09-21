using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using System;

namespace EasyDesk.CleanArchitecture.Domain.Time
{
    public interface ITimestampProvider
    {
        Timestamp Now { get; }
    }

    public static class TimestampProviderExtensions
    {
        public static Duration DurationSince(this ITimestampProvider timestampProvider, Timestamp timestamp)
        {
            var now = timestampProvider.Now;
            if (timestamp > now)
            {
                throw new ArgumentException("The provided timestamp is in the future. Use DurationUntil(timestamp) instead.", nameof(timestamp));
            }
            return Duration.FromTimeOffset(now - timestamp);
        }

        public static Duration DurationUntil(this ITimestampProvider timestampProvider, Timestamp timestamp)
        {
            var now = timestampProvider.Now;
            if (timestamp < now)
            {
                throw new ArgumentException("The provided timestamp is in the past. Use DurationSince(timestamp) instead.", nameof(timestamp));
            }
            return Duration.FromTimeOffset(timestamp - now);
        }
    }
}
