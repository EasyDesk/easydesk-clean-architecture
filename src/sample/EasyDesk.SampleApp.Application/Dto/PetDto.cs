using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

namespace EasyDesk.SampleApp.Application.Dto;

public record PetDto(int Id, string Nickname) : IMappableFrom<Pet, PetDto>
{
    public static PetDto MapFrom(Pet src) => new(
        Id: src.Id,
        Nickname: src.Nickname);
}
