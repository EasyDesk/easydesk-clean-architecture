using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Sender;

public interface IMessageSender : IDisposable
{
    Task Send(IEnumerable<Message> messages);
}
