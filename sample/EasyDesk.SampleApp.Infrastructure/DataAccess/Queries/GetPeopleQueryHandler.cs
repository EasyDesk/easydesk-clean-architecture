using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.SampleApp.Application.Queries;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Queries;

public class GetPeopleQueryHandler : IHandler<GetPeople, Pageable<PersonSnapshot>>
{
    private readonly SampleAppContext _context;
    private readonly IMapper _mapper;

    public GetPeopleQueryHandler(SampleAppContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task<Result<Pageable<PersonSnapshot>>> Handle(GetPeople query)
    {
        return Task.FromResult(Success(_context.People
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ProjectTo<PersonSnapshot>(_mapper.ConfigurationProvider)
            .ToPageable()));
    }
}
