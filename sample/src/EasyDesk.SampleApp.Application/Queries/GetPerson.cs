using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Cqrs;

namespace EasyDesk.SampleApp.Application.Queries;

public static class GetPerson
{
    [RequireAnyOf("People.Read")]
    public record Query(Guid Id) : IQuery<PersonSnapshot>;
}
