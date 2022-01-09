using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.SampleApp.Application.Queries;
using System.Linq;
using System.Threading.Tasks;
using static EasyDesk.SampleApp.Application.Queries.GetPerson;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Queries;

public class GetPersonQueryHandler : RequestHandlerBase<Query, PersonSnapshot>
{
    private readonly SampleAppContext _context;
    private readonly IMapper _mapper;

    public GetPersonQueryHandler(SampleAppContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    protected override async Task<Response<PersonSnapshot>> Handle(Query request)
    {
        return await _context.People
            .Where(p => p.Id == request.Id)
            .ProjectTo<PersonSnapshot>(_mapper.ConfigurationProvider)
            .FirstOptionAsync()
            .ThenOrElseError(Errors.NotFound);
    }
}
