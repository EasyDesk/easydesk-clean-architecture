using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.SampleApp.Application.Dto;

namespace EasyDesk.SampleApp.Application.Queries;

public record GetOwnedPets(Guid PersonId) : IQueryRequest<IPageable<PetDto>>;
