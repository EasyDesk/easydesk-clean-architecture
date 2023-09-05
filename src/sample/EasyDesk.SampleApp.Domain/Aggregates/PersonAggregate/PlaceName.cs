using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using FluentValidation;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

public record PlaceName : PureValue<string, PlaceName>, IValue<string>
{
    public const int MaxLength = 100;

    public record EmptyPlaceName : DomainError;

    public record PlaceNameTooLong(int Length, int MaxLength) : DomainError;

    public PlaceName(string value) : base(value)
    {
    }

    public static string Process(string value) => value.Trim();

    public static IRuleBuilder<T, string> Validate<T>(IRuleBuilder<T, string> value) => value.NotEmpty().MaximumLength(MaxLength);
}
