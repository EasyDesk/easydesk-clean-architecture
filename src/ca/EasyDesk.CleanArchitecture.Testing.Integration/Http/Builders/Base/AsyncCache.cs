namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public sealed class AsyncCache<T>
{
    private Option<T> _cache = None;
    private readonly AsyncFunc<T> _fetch;

    public AsyncCache(AsyncFunc<T> fetch)
    {
        _fetch = fetch;
    }

    public async Task<T> Get() => (_cache || (_cache = Some(await _fetch()))).Value;
}
