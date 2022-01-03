using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;

namespace EasyDesk.CleanArchitecture.Testing;

public class SettableTimestampProvider : ITimestampProvider
{
    public SettableTimestampProvider(Timestamp initial)
    {
        Now = initial;
    }

    public Timestamp Now { get; private set; }

    public SettableTimestampProvider Set(Mapper<Timestamp> mapper)
    {
        Now = mapper(Now);
        return this;
    }

    public SettableTimestampProvider Advance(TimeOffset offset) => Set(t => t + offset);

    public SettableTimestampProvider Advance(Duration duration) => Advance(duration.AsTimeOffset);

    public SettableTimestampProvider AdvanceByMilliseconds(int milliseconds) => Advance(TimeOffset.FromMilliseconds(milliseconds));

    public SettableTimestampProvider AdvanceBySeconds(int seconds) => Advance(TimeOffset.FromSeconds(seconds));

    public SettableTimestampProvider AdvanceByMinutes(int minutes) => Advance(TimeOffset.FromMinutes(minutes));

    public SettableTimestampProvider AdvanceByHours(int hours) => Advance(TimeOffset.FromHours(hours));

    public SettableTimestampProvider AdvanceByDays(int days) => Advance(TimeOffset.FromDays(days));
}
