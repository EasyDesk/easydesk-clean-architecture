using EasyDesk.Tools.PrimitiveTypes.DateAndTime;

namespace EasyDesk.CleanArchitecture.Domain.Time
{
    public interface ITimestampProvider
    {
        Timestamp Now { get; }
    }
}
