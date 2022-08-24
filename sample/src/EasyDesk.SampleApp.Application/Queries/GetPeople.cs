using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Pages;

namespace EasyDesk.SampleApp.Application.Queries;

public static class GetPeople
{
    [RequireAnyOf("People.Read")]
    public record Query(Pagination Pagination) : IPagedQuery<PersonSnapshot>;
}
