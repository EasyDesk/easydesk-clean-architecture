using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public class DomainEventPublisher
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
            .GetMethod(nameof(PublishEventOfType), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(eventType)
            .Invoke(this, [domainEvent]);
        return await (Task<Result<Nothing>>)result!;
    }

    private async Task<Result<Nothing>> PublishEventOfType<T>(T domainEvent)
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
