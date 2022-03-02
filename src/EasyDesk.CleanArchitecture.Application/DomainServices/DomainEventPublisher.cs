using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools;
using EasyDesk.Tools.Results;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using static EasyDesk.Tools.Results.ResultImports;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<Nothing>> Publish(DomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();
        var result = GetType()
            .GetTypeInfo()
            .GetMethod(nameof(PublishEventOfType))
            .MakeGenericMethod(eventType)
            .Invoke(this, new object[] { domainEvent });
        return await (result as Task<Result<Nothing>>);
    }

    public async Task<Result<Nothing>> PublishEventOfType<T>(T domainEvent)
        where T : DomainEvent
    {
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<T>>();
        foreach (var handler in handlers)
        {
            var result = await handler.Handle(domainEvent);
            if (result.IsFailure)
            {
                return result;
            }
        }
        return Ok;
    }
}
