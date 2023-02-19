using EasyDesk.SampleApp.Application.Snapshots;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.People;

public record AddressDto(
    string StreetName,
    string? StreetType = null,
    string? StreetNumber = null,
    string? City = null,
    string? District = null,
    string? Region = null,
    string? State = null,
    string? Country = null)
{
    public AddressValue ToValue() => new(
        StreetType.AsOption(),
        StreetName,
        StreetNumber.AsOption(),
        City.AsOption(),
        District.AsOption(),
        Region.AsOption(),
        State.AsOption(),
        Country.AsOption());

    public static AddressDto From(AddressValue address) => new(
        address.StreetName,
        address.StreetType.OrElseNull(),
        address.StreetNumber.OrElseNull(),
        address.City.OrElseNull(),
        address.District.OrElseNull(),
        address.Region.OrElseNull(),
        address.State.OrElseNull(),
        address.Country.OrElseNull());
}
