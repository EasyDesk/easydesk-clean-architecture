using EasyDesk.CleanArchitecture.Application.Cqrs.Queries;
using EasyDesk.CleanArchitecture.Application.Pagination;

namespace EasyDesk.SampleApp.Application.Queries;

public record GetPeople : IDispatchableQuery<IPageable<PersonSnapshot>>;
