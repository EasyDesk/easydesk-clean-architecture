using EasyDesk.CleanArchitecture.Application.Data;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Receiver;

public abstract class TransactionalMessageHandler<M> : IMessageHandler<M>
    where M : IMessage
{
    public TransactionalMessageHandler()
    {

    }

    public async Task Handle(M message)
    {
        // TODO: implement
        await Handle(message, null);
        throw new NotImplementedException();
    }

    protected abstract Task Handle(M message, IUnitOfWork unitOfWork);
}
