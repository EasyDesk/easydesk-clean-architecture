using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;
using EasyDesk.Commons.Options;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.V_1_0.Dto;

public record AddressDto(
    string StreetName,
    Option<string> StreetType,
    Option<string> StreetNumber,
    Option<string> City,
    Option<string> District,
    Option<string> Province,
    Option<string> Region,
    Option<string> State,
    Option<string> Country) : IObjectValue<AddressDto, Address>
{
    public static AddressDto Create(
        string streetName,
        string? streetType = null,
        string? streetNumber = null,
        string? city = null,
        string? district = null,
        string? province = null,
        string? region = null,
        string? state = null,
        string? country = null) => new(
            streetName,
            streetType.AsOption(),
            streetNumber.AsOption(),
            city.AsOption(),
            district.AsOption(),
            province.AsOption(),
            region.AsOption(),
            state.AsOption(),
            country.AsOption());

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

    public static AddressDto MapFrom(Address address) => new(
        StreetType: address.StreetType.Map(ToValue),
        StreetName: address.StreetName,
        StreetNumber: address.StreetNumber.Map(ToValue),
        City: address.City.Map(ToValue),
        District: address.District.Map(ToValue),
        Province: address.Province.Map(ToValue),
        Region: address.Region.Map(ToValue),
        State: address.State.Map(ToValue),
        Country: address.Country.Map(ToValue));
}

public class AddressDtoValidator : PimpedAbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.StreetName).MustBeValid().For<PlaceName>();
        RuleForOption(x => x.StreetType, r => r.MustBeValid().For<PlaceName>());
        RuleForOption(x => x.StreetNumber, r => r.MustBeValid().For<PlaceName>());
        RuleForOption(x => x.City, r => r.MustBeValid().For<PlaceName>());
        RuleForOption(x => x.District, r => r.MustBeValid().For<PlaceName>());
        RuleForOption(x => x.Province, r => r.MustBeValid().For<PlaceName>());
        RuleForOption(x => x.Region, r => r.MustBeValid().For<PlaceName>());
        RuleForOption(x => x.State, r => r.MustBeValid().For<PlaceName>());
        RuleForOption(x => x.Country, r => r.MustBeValid().For<PlaceName>());
    }
}
