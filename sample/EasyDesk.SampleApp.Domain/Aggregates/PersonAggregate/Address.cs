namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

public record Address(
    Option<PlaceName> StreetType,
    PlaceName StreetName,
    Option<PlaceName> StreetNumber,
    Option<PlaceName> City,
    Option<PlaceName> District,
    Option<PlaceName> Region,
    Option<PlaceName> State,
    Option<PlaceName> Country);
