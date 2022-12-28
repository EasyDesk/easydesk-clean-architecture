namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public abstract class ResponseCache<T>
{
    private Option<T> _cache = None;

    protected abstract Task<T> Fetch();

    protected async Task<T> GetResponseOrCache() => (_cache || (_cache = Some(await Fetch()))).Value;

    public Task<T> Response => GetResponseOrCache();
}
