using EasyDesk.CleanArchitecture.Application.Cqrs;

namespace EasyDesk.SampleApp.Application.Queries;

public static class GetPerson
{
    public record Query(Guid Id) : IQuery<PersonSnapshot>;
}
