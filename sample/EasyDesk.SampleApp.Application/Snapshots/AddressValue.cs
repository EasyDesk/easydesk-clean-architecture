using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.Snapshots;

public record AddressValue(
    Option<string> StreetType,
    string StreetName,
    Option<string> StreetNumber,
    Option<string> City,
    Option<string> District,
    Option<string> Region,
    Option<string> State,
    Option<string> Country)
    : IObjectValue<AddressValue, Address>
{
    public static AddressValue MapFrom(Address address) => new(
        address.StreetType.Map(n => n.Value),
        address.StreetName,
        address.StreetNumber.Map(n => n.Value),
        address.City.Map(n => n.Value),
        address.District.Map(n => n.Value),
        address.Region.Map(n => n.Value),
        address.State.Map(n => n.Value),
        address.Country.Map(n => n.Value));

    public Address ToDomainObject() => new(
        StreetType.Map(n => new PlaceName(n)),
        new PlaceName(StreetName),
        StreetNumber.Map(n => new PlaceName(n)),
        City.Map(n => new PlaceName(n)),
        District.Map(n => new PlaceName(n)),
        Region.Map(n => new PlaceName(n)),
        State.Map(n => new PlaceName(n)),
        Country.Map(n => new PlaceName(n)));
}
