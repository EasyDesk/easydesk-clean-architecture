using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Pagination;

namespace EasyDesk.SampleApp.Application.Queries;

public static class GetPeople
{
    [RequireAnyOf("People.Read")]
    public record Query : IQuery<Pageable<PersonSnapshot>>;
}
