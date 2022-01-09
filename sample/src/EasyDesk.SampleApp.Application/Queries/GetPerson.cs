using EasyDesk.CleanArchitecture.Application.Mediator;
using System;

namespace EasyDesk.SampleApp.Application.Queries;

public static class GetPerson
{
    public record Query(Guid Id) : QueryBase<PersonSnapshot>;
}
