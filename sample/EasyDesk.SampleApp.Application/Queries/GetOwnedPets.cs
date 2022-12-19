using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Pagination;

namespace EasyDesk.SampleApp.Application.Queries;

public record GetOwnedPets(Guid PersonId) : IQueryRequest<IPageable<PetSnapshot>>;
