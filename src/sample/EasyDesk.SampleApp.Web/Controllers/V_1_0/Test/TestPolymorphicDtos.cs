using EasyDesk.Commons.Options;
using NodaTime;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;

public record TestPolymorphicDtos
{
    public required AncestorPolymorphicDto PolymorphicDto { get; init; }

    public required BasePolymorphicDto AbstractPolymorphicDto { get; init; }

    public required PolymorphicExample1 PolymorphicExample1 { get; init; }

    public required IEmptyPolymorphicInterface EmptyPolymorphicInterface { get; init; }
}

public interface IEmptyPolymorphicInterface;

public abstract record AncestorPolymorphicDto
{
    public required string AncestorProperty { get; init; }
}

public record OtherBasePolymorphicDto : AncestorPolymorphicDto
{
    public required string OtherBaseProperty { get; init; }
}

public record BasePolymorphicDto : AncestorPolymorphicDto
{
    public required string BaseProperty { get; init; }
}

public abstract record AbstractPolymorphicDto : BasePolymorphicDto
{
    public required string AbstractBaseProperty { get; init; }
}

public record PolymorphicExample1 : AbstractPolymorphicDto, IEmptyPolymorphicInterface
{
    public required string Property1 { get; init; }

    public required SubRecord SubRecord { get; init; }
}

public record PolymorphicExample2 : AbstractPolymorphicDto, IEmptyPolymorphicInterface
{
    public required int Property2 { get; init; }
}

public record SubRecord(Option<string> SubProperty, string Property, Instant Instant);

public record Record(SubRecord SubRecord, string Property);
