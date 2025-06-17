using Rebus.Timeouts;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus.Scheduler;

public class InMemTimeoutStore
{
    private record Message(DateTimeOffset DueTime, Dictionary<string, string> Headers, byte[] Body);

    private readonly List<Message> _scheduledMessages = [];
    private readonly Lock _lockObject = new();

    public void Defer(DateTimeOffset approximateDueTime, Dictionary<string, string> headers, byte[] body)
    {
        lock (_lockObject)
        {
            var index = _scheduledMessages.FindIndex(m => m.DueTime > approximateDueTime);
            var message = new Message(approximateDueTime, headers, body);
            if (index >= 0)
            {
                _scheduledMessages.Insert(index, message);
            }
            else
            {
                _scheduledMessages.Add(message);
            }
        }
    }

    public DueMessagesResult GetDueMessages(DateTimeOffset now)
    {
        lock (_lockObject)
        {
            var dueMessages = _scheduledMessages
                .TakeWhile(m => m.DueTime <= now)
                .Select(m => new DueMessage(m.Headers, m.Body, () =>
                {
                    lock (_lockObject)
                    {
                        _scheduledMessages.Remove(m);
                        return Task.CompletedTask;
                    }
                }))
                .ToList();

            return new(dueMessages);
        }
    }

    public void Reset()
    {
        lock (_lockObject)
        {
            _scheduledMessages.Clear();
        }
    }
}
