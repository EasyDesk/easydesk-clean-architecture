namespace EasyDesk.CleanArchitecture.Application.Messaging.Inbox;

public interface IInbox
{
    Task<bool> HasBeenProcessed(string messageId);

    Task MarkAsProcessed(string messageId);
}
