using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.Queries;

public record AddressValue(
    string StreetType,
    string StreetName,
    string StreetNumber,
    string City,
    string District,
    string Region,
    string State,
    string Country)
{
    public static AddressValue MapFrom(Address address) => new(
        address.StreetType,
        address.StreetName,
        address.StreetNumber,
        address.City,
        address.District,
        address.Region,
        address.State,
        address.Country);

    public Address MapToDomain() => new(
        new PlaceName(StreetType),
        new PlaceName(StreetName),
        new PlaceName(StreetNumber),
        new PlaceName(City),
        new PlaceName(District),
        new PlaceName(Region),
        new PlaceName(State),
        new PlaceName(Country));
}
