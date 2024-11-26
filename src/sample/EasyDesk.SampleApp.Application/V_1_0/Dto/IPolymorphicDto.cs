namespace EasyDesk.SampleApp.Application.V_1_0.Dto;

public interface IPolymorphicDto;

public record PolymorphicExample1 : IPolymorphicDto
{
    public required string Property1 { get; init; }
}

public record PolymorphicExample2 : IPolymorphicDto
{
    public required int Property2 { get; init; }
}
