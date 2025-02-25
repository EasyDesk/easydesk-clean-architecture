using EasyDesk.Commons.Collections;

namespace EasyDesk.Commons.Scopes;

public sealed class ScopeManager<T>
{
    private readonly T _baseValue;
    private readonly Stack<Scope> _scopeStack = new();

    public ScopeManager(T baseValue)
    {
        _baseValue = baseValue;
    }

    public T Current => _scopeStack.IsEmpty() ? _baseValue : _scopeStack.Peek().Value;

    public int Depth => _scopeStack.Count;

    public Scope OpenScope(T value)
    {
        var scope = new Scope(value, this);
        _scopeStack.Push(scope);
        return scope;
    }

    public sealed class Scope : IDisposable
    {
        private readonly ScopeManager<T> _manager;

        public Scope(T value, ScopeManager<T> manager)
        {
            Value = value;
            _manager = manager;
        }

        public T Value { get; }

        public void Dispose()
        {
            if (_manager._scopeStack.IsEmpty() || _manager._scopeStack.Pop() != this)
            {
                throw new InvalidOperationException("Detected incorrect scope disposal.");
            }
        }
    }
}
