using Rebus.Time;
using Rebus.Timeouts;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus.Scheduler;

public class InMemTimeoutManager : ITimeoutManager
{
    private readonly InMemTimeoutStore _timeoutStore;
    private readonly IRebusTime _rebusTime;

    public InMemTimeoutManager(InMemTimeoutStore timeoutStore, IRebusTime rebusTime)
    {
        _timeoutStore = timeoutStore;
        _rebusTime = rebusTime;
    }

    public Task Defer(DateTimeOffset approximateDueTime, Dictionary<string, string> headers, byte[] body)
    {
        _timeoutStore.Defer(approximateDueTime, headers, body);
        return Task.CompletedTask;
    }

    public Task<DueMessagesResult> GetDueMessages()
    {
        return Task.FromResult(_timeoutStore.GetDueMessages(_rebusTime.Now));
    }
}
