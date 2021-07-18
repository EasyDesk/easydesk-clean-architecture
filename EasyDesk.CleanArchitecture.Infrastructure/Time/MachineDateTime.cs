using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;

namespace EasyDesk.CleanArchitecture.Infrastructure.Time
{
    public class MachineDateTime : ITimestampProvider
    {
        public Timestamp Now => Timestamp.Now;
    }
}
