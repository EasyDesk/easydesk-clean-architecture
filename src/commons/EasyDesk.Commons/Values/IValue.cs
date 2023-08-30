using EasyDesk.Commons.Options;

namespace EasyDesk.Commons.Values;

public interface IValue<TSelf, TWrapped>
    where TSelf : IValue<TSelf, TWrapped>
{
    static abstract TSelf New(TWrapped value);

    static abstract Option<TSelf> TryNew(TWrapped value);

    TWrapped Value { get; }
}
