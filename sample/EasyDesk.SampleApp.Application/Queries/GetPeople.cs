using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Pagination;

namespace EasyDesk.SampleApp.Application.Queries;

public static class GetPeople
{
    public record Query : IQuery<Pageable<PersonSnapshot>>;
}
