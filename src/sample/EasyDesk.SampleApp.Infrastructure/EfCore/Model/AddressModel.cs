using EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Model;

public class AddressModel
    : IValuePersistence<Address, AddressModel, Address>
{
    required public string? StreetType { get; set; }

    required public string StreetName { get; set; }

    required public string? StreetNumber { get; set; }

    required public string? City { get; set; }

    required public string? District { get; set; }

    required public string? Province { get; set; }

    required public string? Region { get; set; }

    required public string? State { get; set; }

    required public string? Country { get; set; }

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

    public static void ApplyChanges(Address origin, AddressModel destination)
    {
        destination.StreetType = origin.StreetType.Map(ToValue).OrElseNull();
        destination.StreetName = origin.StreetName;
        destination.StreetNumber = origin.StreetNumber.Map(ToValue).OrElseNull();
        destination.City = origin.City.Map(ToValue).OrElseNull();
        destination.District = origin.District.Map(ToValue).OrElseNull();
        destination.Province = origin.Province.Map(ToValue).OrElseNull();
        destination.Region = origin.Region.Map(ToValue).OrElseNull();
        destination.State = origin.State.Map(ToValue).OrElseNull();
        destination.Country = origin.Country.Map(ToValue).OrElseNull();
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
