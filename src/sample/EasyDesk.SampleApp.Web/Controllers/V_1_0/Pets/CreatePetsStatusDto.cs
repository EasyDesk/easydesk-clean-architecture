using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.SampleApp.Application.Queries;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;

public record CreatePetsStatusDto(bool InProgress) : IMappableFrom<BulkCreatePetsStatus, CreatePetsStatusDto>
{
    public static CreatePetsStatusDto MapFrom(BulkCreatePetsStatus src) => new(src.InProgress);
}
