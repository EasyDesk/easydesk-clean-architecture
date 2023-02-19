using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.SampleApp.Application.Snapshots;

namespace EasyDesk.SampleApp.Application.Queries;

public record GetPeople : IQueryRequest<IPageable<PersonSnapshot>>;
