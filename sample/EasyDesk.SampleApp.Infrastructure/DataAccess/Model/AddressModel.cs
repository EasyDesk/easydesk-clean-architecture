using EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using System.Linq.Expressions;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

public record AddressModel(
    string StreetType,
    string StreetName,
    string StreetNumber,
    string City,
    string District,
    string Region,
    string State,
    string Country)
    : IPersistenceObject<Address, AddressModel>
{
    public static Expression<Func<AddressModel, AddressValue>> Projection() => src => new(
        src.StreetType,
        src.StreetName,
        src.StreetNumber,
        src.City,
        src.District,
        src.Region,
        src.State,
        src.Country);

    public static AddressModel ToPersistence(Address origin) => new(
        origin.StreetType,
        origin.StreetName,
        origin.StreetNumber,
        origin.City,
        origin.District,
        origin.Region,
        origin.State,
        origin.Country);

    public Address ToDomain() => new(
        new PlaceName(StreetType),
        new PlaceName(StreetName),
        new PlaceName(StreetNumber),
        new PlaceName(City),
        new PlaceName(District),
        new PlaceName(Region),
        new PlaceName(State),
        new PlaceName(Country));
}
