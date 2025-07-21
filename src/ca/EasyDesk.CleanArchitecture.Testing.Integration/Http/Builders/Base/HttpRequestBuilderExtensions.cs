using EasyDesk.CleanArchitecture.Web.Dto;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public static class HttpRequestBuilderExtensions
{
    public static HttpRequestBuilder PageIndex(this HttpRequestBuilder builder, int pageIndex) =>
        PageIndex(builder, pageIndex.ToString());

    public static HttpRequestBuilder PageSize(this HttpRequestBuilder builder, int pageSize) =>
        PageSize(builder, pageSize.ToString());

    public static HttpRequestBuilder PageIndex(this HttpRequestBuilder builder, string pageIndex) =>
        builder.Query(x => x.Replace(nameof(PaginationDto.PageIndex), pageIndex));

    public static HttpRequestBuilder PageSize(this HttpRequestBuilder builder, string pageSize) =>
        builder.Query(x => x.Replace(nameof(PaginationDto.PageSize), pageSize));

    public static HttpRequestBuilder RemovePageIndex(this HttpRequestBuilder builder) =>
        builder.Query(x => x.Remove(nameof(PaginationDto.PageIndex)));

    public static HttpRequestBuilder RemovePageSize(this HttpRequestBuilder builder) =>
        builder.Query(x => x.Remove(nameof(PaginationDto.PageSize)));
}
