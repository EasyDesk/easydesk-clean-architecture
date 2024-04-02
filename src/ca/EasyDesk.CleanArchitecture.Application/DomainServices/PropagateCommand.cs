using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public sealed class PropagateCommand<M, D> : IDomainEventHandler<D>
    where M : IPropagatedCommand<M, D>, IOutgoingCommand
    where D : DomainEvent
{
    private readonly ICommandSender _commandSender;
    private readonly IContextTenantNavigator? _tenantNavigator;

    public PropagateCommand(
        ICommandSender commandSender,
        IContextTenantNavigator? tenantNavigator = null)
    {
        _commandSender = commandSender;
        _tenantNavigator = tenantNavigator;
    }

    public async Task<Result<Nothing>> Handle(D ev)
    {
        await M.ToTenant(ev).MatchAsync(
            some: async t =>
            {
                using var scope = _tenantNavigator?.NavigateTo(t);
                await Propagate(ev);
            },
            none: async () => await Propagate(ev));

        return Ok;
    }

    private async Task Propagate(D ev)
    {
        var options = new PropagatedCommandOptions<M>();
        M.ConfigureOptions(ev, options);
        await options.Propagate(M.ToMessage(ev), _commandSender);
    }
}
