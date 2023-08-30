using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.Queries;
using EasyDesk.SampleApp.Infrastructure.EfCore.Model;

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Queries;

public class GetOwnedPetsQueryHandler : IHandler<GetOwnedPets, IPageable<PetDto>>
{
    private readonly SampleAppContext _context;

    public GetOwnedPetsQueryHandler(SampleAppContext context)
    {
        _context = context;
    }

    public Task<Result<IPageable<PetDto>>> Handle(GetOwnedPets request)
    {
        return Task.FromResult(Success(_context.Pets
            .Where(p => p.PersonId == request.PersonId)
            .OrderBy(p => p.Id)
            .Project<PetModel, PetDto>()
            .ToPageable()));
    }
}
