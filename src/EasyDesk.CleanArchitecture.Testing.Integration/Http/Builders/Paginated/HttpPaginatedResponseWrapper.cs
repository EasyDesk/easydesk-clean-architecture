using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Web.Dto;
using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;

public class HttpPaginatedResponseWrapper<T> : HttpResponseWrapper<IEnumerable<T>, PaginationMetaDto>
{
    public HttpPaginatedResponseWrapper(HttpResponseMessage httpResponseMessage, JsonSerializerSettings jsonSerializerSettings)
        : base(httpResponseMessage, jsonSerializerSettings)
    {
    }

    public HttpPaginatedResponseWrapper(AsyncFunc<HttpResponseMessage> httpResponseMessage, JsonSerializerSettings jsonSerializerSettings)
        : base(httpResponseMessage, jsonSerializerSettings)
    {
    }

    public async Task<(int, int)> PageIndexAndCount()
    {
        var m = await AsMetadata();
        return (m.PageIndex, m.PageCount);
    }
}
