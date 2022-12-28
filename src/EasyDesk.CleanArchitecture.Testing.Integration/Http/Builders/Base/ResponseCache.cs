namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public abstract class ResponseCache<T>
{
    private Option<T> _cache = None;
    private readonly AsyncFunc<T> _fetch;

    public ResponseCache(AsyncFunc<T> fetch)
    {
        _fetch = fetch;
    }

    public async Task<T> GetResponse() => (_cache || (_cache = Some(await _fetch()))).Value;
}
