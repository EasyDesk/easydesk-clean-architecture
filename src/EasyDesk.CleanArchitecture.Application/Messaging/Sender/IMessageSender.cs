using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Sender;

public interface IMessageSender
{
    Task Defer(Timestamp time, IMessage message);

    Task Defer(Duration delay, IMessage message);

    Task DeferLocal(Timestamp time, IMessage message);

    Task DeferLocal(Duration delay, IMessage message);

    Task Publish(IMessage eventMessage);

    Task Send(IMessage commandMessage);

    Task SendLocal(IMessage commandMessage);
}
