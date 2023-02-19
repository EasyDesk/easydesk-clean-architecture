using EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;
using EasyDesk.SampleApp.Application.Snapshots;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using System.Linq.Expressions;

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
    : IDomainPersistence<Address, AddressModel>, IProjectable<AddressModel, AddressValue>
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

    public AddressValue ToProjection() => Projection().Compile().Invoke(this);

    public static Expression<Func<AddressModel, AddressValue>> Projection() => src => new(
        src.StreetType.AsOption(),
        src.StreetName,
        src.StreetNumber.AsOption(),
        src.City.AsOption(),
        src.District.AsOption(),
        src.Region.AsOption(),
        src.State.AsOption(),
        src.Country.AsOption());
}
