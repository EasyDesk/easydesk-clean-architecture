using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

public class AddressModel
    : IMutablePersistence<Address, AddressModel>
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
        StreetType.AsOption().Map(n => new PlaceName(n)),
        new PlaceName(StreetName),
        StreetNumber.AsOption().Map(n => new PlaceName(n)),
        City.AsOption().Map(n => new PlaceName(n)),
        Province.AsOption().Map(n => new PlaceName(n)),
        District.AsOption().Map(n => new PlaceName(n)),
        Region.AsOption().Map(n => new PlaceName(n)),
        State.AsOption().Map(n => new PlaceName(n)),
        Country.AsOption().Map(n => new PlaceName(n)));

    public static void ApplyChanges(Address origin, AddressModel destination)
    {
        destination.StreetType = origin.StreetType.Map(ToValue).OrElseNull();
        destination.StreetName = origin.StreetName;
        destination.StreetNumber = origin.StreetNumber.Map(ToValue).OrElseNull();
        destination.City = origin.City.Map(ToValue).OrElseNull();
        destination.Province = origin.City.Map(ToValue).OrElseNull();
        destination.District = origin.District.Map(ToValue).OrElseNull();
        destination.Region = origin.Region.Map(ToValue).OrElseNull();
        destination.State = origin.State.Map(ToValue).OrElseNull();
        destination.Country = origin.Country.Map(ToValue).OrElseNull();
    }
}
