using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using NodaTime;

namespace EasyDesk.SampleApp.Application.Dto;

public record PersonDto(
    Guid Id,
    string FirstName,
    string LastName,
    LocalDate DateOfBirth,
    string CreatedBy,
    AddressDto Residence,
    bool Approved) : IMappableFrom<Person, PersonDto>
{
    public static PersonDto MapFrom(Person src) => new(
        src.Id,
        src.FirstName,
        src.LastName,
        src.DateOfBirth,
        src.CreatedBy,
        AddressDto.MapFrom(src.Residence),
        src.Approved);
}
