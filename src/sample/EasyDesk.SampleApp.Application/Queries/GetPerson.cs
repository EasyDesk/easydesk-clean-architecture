using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.SampleApp.Application.Dto;

namespace EasyDesk.SampleApp.Application.Queries;

public record GetPerson(Guid Id) : IQueryRequest<PersonDto>;
