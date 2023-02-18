using EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

public record AddressModel(
    string? StreetType,
    string StreetName,
    string? StreetNumber,
    string? City,
    string? District,
    string? Region,
    string? State,
    string? Country)
    : IPersistenceObject<Address, AddressModel>
{
    public static AddressModel ToPersistence(Address origin) => new(
        origin.StreetType.Map(n => n.Value).OrElseNull(),
        origin.StreetName,
        origin.StreetNumber.Map(n => n.Value).OrElseNull(),
        origin.City.Map(n => n.Value).OrElseNull(),
        origin.District.Map(n => n.Value).OrElseNull(),
        origin.Region.Map(n => n.Value).OrElseNull(),
        origin.State.Map(n => n.Value).OrElseNull(),
        origin.Country.Map(n => n.Value).OrElseNull());

    public Address ToDomain() => new(
        StreetType.AsOption().Map(n => new PlaceName(n)),
        new PlaceName(StreetName),
        StreetNumber.AsOption().Map(n => new PlaceName(n)),
        City.AsOption().Map(n => new PlaceName(n)),
        District.AsOption().Map(n => new PlaceName(n)),
        Region.AsOption().Map(n => new PlaceName(n)),
        State.AsOption().Map(n => new PlaceName(n)),
        Country.AsOption().Map(n => new PlaceName(n)));

    public AddressValue Projection() => new(
        StreetType.AsOption(),
        StreetName,
        StreetNumber.AsOption(),
        City.AsOption(),
        District.AsOption(),
        Region.AsOption(),
        State.AsOption(),
        Country.AsOption());
}
