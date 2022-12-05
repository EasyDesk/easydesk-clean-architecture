using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;

namespace EasyDesk.SampleApp.Application.Queries;

public record GetPerson(Guid Id) : IQueryRequest<PersonSnapshot>;
