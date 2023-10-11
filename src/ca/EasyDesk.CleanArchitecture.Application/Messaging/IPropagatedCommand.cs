using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Tasks;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IPropagatedCommand<M, D>
    where M : IPropagatedCommand<M, D>, IOutgoingCommand
    where D : DomainEvent
{
    static abstract M ToMessage(D domainEvent);

    static virtual void ConfigureOptions(D domainEvent, PropagatedCommandOptions<M> options)
    {
    }

    static virtual Option<TenantInfo> ToTenant(D domainEvent) => None;
}

public class PropagatedCommandOptions<M>
    where M : IOutgoingCommand
{
    private static readonly AsyncAction<ICommandSender, M> _sendImmediately = (s, m) => s.Send(m);

    private AsyncAction<ICommandSender, M> _propagateAction = _sendImmediately;

    public PropagatedCommandOptions<M> SendImmediately()
    {
        _propagateAction = _sendImmediately;
        return this;
    }

    public PropagatedCommandOptions<M> Defer(Duration delay)
    {
        _propagateAction = (s, m) => s.Defer(delay, m);
        return this;
    }

    public PropagatedCommandOptions<M> Schedule(Instant instant)
    {
        _propagateAction = (s, m) => s.Schedule(instant, m);
        return this;
    }

    internal async Task Propagate(M message, ICommandSender commandSender)
    {
        await _propagateAction(commandSender, message);
    }
}
