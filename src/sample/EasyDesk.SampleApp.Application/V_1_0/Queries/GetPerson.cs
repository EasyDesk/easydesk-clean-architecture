using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.SampleApp.Application.V_1_0.Dto;

namespace EasyDesk.SampleApp.Application.V_1_0.Queries;

public record GetPerson(Guid Id) : IQueryRequest<PersonDto>;
