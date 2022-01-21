using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker;

public interface IMessagePublisher : IDisposable
{
    Task Publish(IEnumerable<Message> messages);
}
