using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Mediator;
using System;

namespace EasyDesk.SampleApp.Application.Queries;

public static class GetPerson
{
    [RequireAnyOf("People.Read")]
    public record Query(Guid Id) : QueryBase<PersonSnapshot>;
}
