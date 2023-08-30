using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Model;

public class AddressModel
    : IValuePersistence<Address, AddressModel, Address>
{
    public required string? StreetType { get; set; }

    public required string StreetName { get; set; }

    public required string? StreetNumber { get; set; }

    public required string? City { get; set; }

    public required string? District { get; set; }

    public required string? Province { get; set; }

    public required string? Region { get; set; }

    public required string? State { get; set; }

    public required string? Country { get; set; }

    public Address ToDomain() => new(
        StreetType: StreetType.AsOption().Map(n => new PlaceName(n)),
        StreetName: new PlaceName(StreetName),
        StreetNumber: StreetNumber.AsOption().Map(n => new PlaceName(n)),
        City: City.AsOption().Map(n => new PlaceName(n)),
        District: District.AsOption().Map(n => new PlaceName(n)),
        Province: Province.AsOption().Map(n => new PlaceName(n)),
        Region: Region.AsOption().Map(n => new PlaceName(n)),
        State: State.AsOption().Map(n => new PlaceName(n)),
        Country: Country.AsOption().Map(n => new PlaceName(n)));

    public void ApplyChanges(Address origin)
    {
        StreetType = origin.StreetType.Map(ToValue).OrElseNull();
        StreetName = origin.StreetName;
        StreetNumber = origin.StreetNumber.Map(ToValue).OrElseNull();
        City = origin.City.Map(ToValue).OrElseNull();
        District = origin.District.Map(ToValue).OrElseNull();
        Province = origin.Province.Map(ToValue).OrElseNull();
        Region = origin.Region.Map(ToValue).OrElseNull();
        State = origin.State.Map(ToValue).OrElseNull();
        Country = origin.Country.Map(ToValue).OrElseNull();
    }

    public static AddressModel ToPersistence(Address origin) => new()
    {
        StreetType = origin.StreetType.Map(ToValue).OrElseNull(),
        StreetName = origin.StreetName,
        StreetNumber = origin.StreetNumber.Map(ToValue).OrElseNull(),
        City = origin.City.Map(ToValue).OrElseNull(),
        District = origin.District.Map(ToValue).OrElseNull(),
        Province = origin.Province.Map(ToValue).OrElseNull(),
        Region = origin.Region.Map(ToValue).OrElseNull(),
        State = origin.State.Map(ToValue).OrElseNull(),
        Country = origin.Country.Map(ToValue).OrElseNull(),
    };
}
