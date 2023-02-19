using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.SampleApp.Application.Snapshots;

namespace EasyDesk.SampleApp.Application.Queries;

public record GetPerson(Guid Id) : IQueryRequest<PersonSnapshot>;
