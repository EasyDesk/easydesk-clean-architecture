using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;

namespace EasyDesk.CleanArchitecture.Infrastructure.Time
{
    public class FixedDateTime : ITimestampProvider
    {
        public FixedDateTime(Timestamp now)
        {
            Now = now;
        }

        public Timestamp Now { get; }
    }
}
