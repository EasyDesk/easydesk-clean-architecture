using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.Snapshots;

public record AddressValue(
    Option<string> StreetType,
    string StreetName,
    Option<string> StreetNumber,
    Option<string> City,
    Option<string> District,
    Option<string> Province,
    Option<string> Region,
    Option<string> State,
    Option<string> Country)
    : IObjectValue<AddressValue, Address>
{
    public static AddressValue MapFrom(Address address) => new(
        address.StreetType.Map(ToValue),
        address.StreetName,
        address.StreetNumber.Map(ToValue),
        address.City.Map(ToValue),
        address.District.Map(ToValue),
        address.Province.Map(ToValue),
        address.Region.Map(ToValue),
        address.State.Map(ToValue),
        address.Country.Map(ToValue));

    public Address ToDomainObject() => new(
        StreetType.Map(n => new PlaceName(n)),
        new PlaceName(StreetName),
        StreetNumber.Map(n => new PlaceName(n)),
        City.Map(n => new PlaceName(n)),
        District.Map(n => new PlaceName(n)),
        Province.Map(n => new PlaceName(n)),
        Region.Map(n => new PlaceName(n)),
        State.Map(n => new PlaceName(n)),
        Country.Map(n => new PlaceName(n)));
}
