using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.SampleApp.Application.Queries;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Queries;

public class GetPersonQueryHandler : IHandler<GetPerson, PersonSnapshot>
{
    private readonly SampleAppContext _context;
    private readonly IMapper _mapper;

    public GetPersonQueryHandler(SampleAppContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PersonSnapshot>> Handle(GetPerson request)
    {
        return await _context.People
            .Where(p => p.Id == request.Id)
            .ProjectTo<PersonSnapshot>(_mapper.ConfigurationProvider)
            .FirstOptionAsync()
            .ThenOrElseError(Errors.NotFound);
    }
}
