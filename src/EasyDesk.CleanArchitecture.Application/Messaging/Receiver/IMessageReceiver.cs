using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Receiver;

public interface IMessageReceiver : IDisposable
{
    Task Start();
}
