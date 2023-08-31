using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;

public sealed class HttpPageSequenceWrapper<T>
{
    private readonly AsyncCache<IEnumerable<HttpResponseWrapper<T, PaginationMetaDto>>> _response;

    public HttpPageSequenceWrapper(AsyncFunc<IEnumerable<HttpResponseWrapper<T, PaginationMetaDto>>> responses)
    {
        _response = new(responses);
    }

    public async Task EnsureSuccess()
    {
        foreach (var page in await GetPages())
        {
            await page.EnsureSuccess();
        }
    }

    public async Task EnsureFailure()
    {
        foreach (var page in await GetPages())
        {
            await page.EnsureFailure();
        }
    }

    public async Task<IEnumerable<HttpResponseWrapper<T, PaginationMetaDto>>> GetPages() => await _response.Get();
}

public static class HttpPageSequenceWrapperExtensions
{
    public static async Task<IEnumerable<T>> AsVerifiableEnumerable<T>(this HttpPageSequenceWrapper<IEnumerable<T>> wrapper)
    {
        var result = new List<T>();
        foreach (var page in await wrapper.GetPages())
        {
            result.AddRange(await page.AsData());
        }
        return result;
    }

    public static async Task Verify<T>(this HttpPageSequenceWrapper<IEnumerable<T>> wrapper, Action<SettingsTask>? configure = null)
    {
        var response = await wrapper.AsVerifiableEnumerable();
        var settingsTask = Verifier.Verify(response);
        configure?.Invoke(settingsTask);
        await settingsTask;
    }
}
