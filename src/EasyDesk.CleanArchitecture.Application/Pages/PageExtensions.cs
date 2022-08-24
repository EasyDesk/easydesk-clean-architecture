namespace EasyDesk.CleanArchitecture.Application.Pages;

public static class PageExtensions
{
    public static Page<B> Map<A, B>(this Page<A> page, Func<A, B> mapper) => new(page.Items.Select(mapper), page.Pagination, page.Count);

    public static Task<Page<B>> MapPage<A, B>(this Task<Page<A>> task, Func<A, B> mapper) => task.Map(p => p.Map(mapper));
}
