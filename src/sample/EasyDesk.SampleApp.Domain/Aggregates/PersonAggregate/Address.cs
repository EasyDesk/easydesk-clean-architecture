using EasyDesk.Commons.Options;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

public record Address(
    Option<PlaceName> StreetType,
    PlaceName StreetName,
    Option<PlaceName> StreetNumber,
    Option<PlaceName> City,
    Option<PlaceName> District,
    Option<PlaceName> Province,
    Option<PlaceName> Region,
    Option<PlaceName> State,
    Option<PlaceName> Country)
{
    public static Address Create(
        PlaceName streetName,
        PlaceName? streetType = null,
        PlaceName? streetNumber = null,
        PlaceName? city = null,
        PlaceName? district = null,
        PlaceName? province = null,
        PlaceName? region = null,
        PlaceName? state = null,
        PlaceName? country = null)
    {
        return new(
            StreetType: streetType.AsOption(),
            StreetName: streetName,
            StreetNumber: streetNumber.AsOption(),
            City: city.AsOption(),
            District: district.AsOption(),
            Province: province.AsOption(),
            Region: region.AsOption(),
            State: state.AsOption(),
            Country: country.AsOption());
    }
}
