using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Scopes;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public class DomainEventScope : IDomainEventNotifier
{
    private readonly ScopeManager<DomainEventQueue> _scopeManager;
    private readonly DomainEventPublisher _publisher;
    private readonly ISaveChangesHandler _saveChangesHandler;

    public DomainEventScope(DomainEventPublisher publisher, ISaveChangesHandler saveChangesHandler)
    {
        _publisher = publisher;
        _saveChangesHandler = saveChangesHandler;
        _scopeManager = new(new(publisher, saveChangesHandler));
    }

    public void Notify(DomainEvent domainEvent) => _scopeManager.Current.Notify(domainEvent);

    public async Task<Result<R>> RunInScope<R>(AsyncFunc<Result<R>> action)
    {
        using var scope = _scopeManager.OpenScope(new(_publisher, _saveChangesHandler));
        return await action().ThenFlatTapAsync(_ => scope.Value.Flush());
    }
}
