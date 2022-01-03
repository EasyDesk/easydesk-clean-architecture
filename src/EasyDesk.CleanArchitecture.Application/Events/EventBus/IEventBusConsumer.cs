using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus;

public interface IEventBusConsumer : IDisposable
{
    Task StartListening();
}
