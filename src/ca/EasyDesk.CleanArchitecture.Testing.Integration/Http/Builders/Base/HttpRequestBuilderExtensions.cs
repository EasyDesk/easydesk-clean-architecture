using EasyDesk.CleanArchitecture.Web.Dto;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public static class HttpRequestBuilderExtensions
{
    public static B SetPageIndex<B>(this HttpRequestBuilder<B> builder, int pageIndex)
        where B : HttpRequestBuilder<B> =>
        SetPageIndex(builder, pageIndex.ToString());

    public static B SetPageSize<B>(this HttpRequestBuilder<B> builder, int pageSize)
        where B : HttpRequestBuilder<B> =>
        SetPageSize(builder, pageSize.ToString());

    public static B SetPageIndex<B>(this HttpRequestBuilder<B> builder, string pageIndex)
        where B : HttpRequestBuilder<B> =>
        builder.WithQuery(nameof(PaginationDto.PageIndex), pageIndex);

    public static B SetPageSize<B>(this HttpRequestBuilder<B> builder, string pageSize)
        where B : HttpRequestBuilder<B> =>
        builder.WithQuery(nameof(PaginationDto.PageSize), pageSize);

    public static B RemovePageIndex<B>(this HttpRequestBuilder<B> builder)
        where B : HttpRequestBuilder<B> =>
        builder.WithoutQuery(nameof(PaginationDto.PageIndex));

    public static B RemovePageSize<B>(this HttpRequestBuilder<B> builder)
        where B : HttpRequestBuilder<B> =>
        builder.WithoutQuery(nameof(PaginationDto.PageSize));
}
