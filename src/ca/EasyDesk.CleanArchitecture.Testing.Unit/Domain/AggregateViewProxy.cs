using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Testing.Unit.Domain;

public class AggregateViewProxy<T> : IAggregateView<T>
{
    private readonly Option<T> _result;

    public AggregateViewProxy(Option<T> result)
    {
        _result = result;
    }

    public Task<Option<T>> AsOption() => Task.FromResult(_result);

    public Task<bool> Exists() => Task.FromResult(_result.IsPresent);
}
