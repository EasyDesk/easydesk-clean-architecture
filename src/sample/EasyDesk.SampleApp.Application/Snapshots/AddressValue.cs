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
        StreetType: address.StreetType.Map(ToValue),
        StreetName: address.StreetName,
        StreetNumber: address.StreetNumber.Map(ToValue),
        City: address.City.Map(ToValue),
        District: address.District.Map(ToValue),
        Province: address.Province.Map(ToValue),
        Region: address.Region.Map(ToValue),
        State: address.State.Map(ToValue),
        Country: address.Country.Map(ToValue));

    public Address ToDomainObject() => new(
        StreetType: StreetType.Map(n => new PlaceName(n)),
        StreetName: new PlaceName(StreetName),
        StreetNumber: StreetNumber.Map(n => new PlaceName(n)),
        City: City.Map(n => new PlaceName(n)),
        District: District.Map(n => new PlaceName(n)),
        Province: Province.Map(n => new PlaceName(n)),
        Region: Region.Map(n => new PlaceName(n)),
        State: State.Map(n => new PlaceName(n)),
        Country: Country.Map(n => new PlaceName(n)));
}
