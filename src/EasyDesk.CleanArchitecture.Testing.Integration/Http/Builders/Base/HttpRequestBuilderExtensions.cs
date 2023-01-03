using EasyDesk.CleanArchitecture.Web.Dto;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public static class HttpRequestBuilderExtensions
{
    public static B SetPageIndex<B>(this HttpRequestBuilder<B> builder, int pageIndex)
        where B : HttpRequestBuilder<B> =>
        builder.WithQuery(nameof(PaginationDto.PageIndex), pageIndex.ToString());

    public static B SetPageSize<B>(this HttpRequestBuilder<B> builder, int pageIndex)
        where B : HttpRequestBuilder<B> =>
        builder.WithQuery(nameof(PaginationDto.PageSize), pageIndex.ToString());
}
