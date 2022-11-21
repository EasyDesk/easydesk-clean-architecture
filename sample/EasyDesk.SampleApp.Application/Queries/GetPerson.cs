using EasyDesk.CleanArchitecture.Application.Cqrs.Queries;

namespace EasyDesk.SampleApp.Application.Queries;

public record GetPerson(Guid Id) : IDispatchableQuery<PersonSnapshot>;
