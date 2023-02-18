using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

public record PlaceName : ValueWrapper<string, PlaceName>
{
    public const int MaxLength = 100;

    public record EmptyPlaceName : DomainError;

    public record PlaceNameTooLong(int Length, int MaxLength) : DomainError;

    public PlaceName(string value) : base(value)
    {
        DomainConstraints.Check()
            .If(string.IsNullOrWhiteSpace(value), () => new EmptyPlaceName())
            .ThrowException();
        DomainConstraints.Check()
            .IfNot(value.Length < MaxLength, () => new PlaceNameTooLong(value.Length, MaxLength))
            .ThrowException();
    }
}
