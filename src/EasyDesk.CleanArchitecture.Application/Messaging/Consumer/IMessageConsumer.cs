using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker;

public interface IMessageConsumer : IDisposable
{
    Task StartListening();
}
