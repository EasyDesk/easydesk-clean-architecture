﻿using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Receiver;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Receiver;

public class TransactionalMessageHandler : IMessageHandler
{
    private readonly IMessageHandler _handler;
    private readonly ITransactionManager _transactionManager;

    public TransactionalMessageHandler(IMessageHandler handler, ITransactionManager transactionManager)
    {
        _handler = handler;
        _transactionManager = transactionManager;
    }

    public async Task<MessageHandlerResult> Handle(Message message)
    {
        await _transactionManager.Begin();
        var handlerResult = await _handler.Handle(message);
        if (handlerResult is MessageHandlerResult.Handled)
        {
            var commitResult = await _transactionManager.Commit();
            return commitResult.Match(
                success: _ => handlerResult,
                failure: _ => MessageHandlerResult.TransientFailure);
        }
        return handlerResult;
    }
}