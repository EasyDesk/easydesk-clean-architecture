using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Application.Snapshots;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Queries;

public class GetOwnedPetsQueryHandler : IHandler<GetOwnedPets, IPageable<PetSnapshot>>
{
    private readonly SampleAppContext _context;

    public GetOwnedPetsQueryHandler(SampleAppContext context)
    {
        _context = context;
    }

    public Task<Result<IPageable<PetSnapshot>>> Handle(GetOwnedPets request)
    {
        return Task.FromResult(Success(_context.Pets
            .Where(p => p.PersonId == request.PersonId)
            .OrderBy(p => p.Id)
            .Project<PetModel, PetSnapshot>()
            .ToPageable()));
    }
}
