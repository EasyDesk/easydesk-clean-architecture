﻿using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.Queries;
using EasyDesk.SampleApp.Infrastructure.EfCore.Model;

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Queries;

public class GetPeopleQueryHandler : SuccessHandler<GetPeople, IPageable<PersonDto>>
{
    private readonly SampleAppContext _context;

    public GetPeopleQueryHandler(SampleAppContext context)
    {
        _context = context;
    }

    protected override Task<IPageable<PersonDto>> Process(GetPeople request)
    {
        return Task.FromResult(_context.People
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Project<PersonModel, PersonDto>()
            .ToPageable());
    }
}
