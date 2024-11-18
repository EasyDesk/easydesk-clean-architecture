using EasyDesk.CleanArchitecture.Application.Json.Converters;
using EasyDesk.Commons.Options;
using Shouldly;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Json.Converters;

public class OptionConverterTests
{
    private record TestRecord(int Value);

    private readonly JsonSerializerOptions _options;

    private readonly OptionConverter _sut = new();
    private readonly TestRecord _record = new(10);

    public OptionConverterTests()
    {
        _options = new JsonSerializerOptions().Also(x => x.Converters.Add(_sut));
    }

    private string SerializeOption(Option<TestRecord> value) => JsonSerializer.Serialize(value, _options);

    private Option<TestRecord> DeserializeOption(string json) => JsonSerializer.Deserialize<Option<TestRecord>>(json, _options);

    [Fact]
    public void ShouldSerializeNoneAsANullToken()
    {
        SerializeOption(None).ShouldBe(JsonSerializer.Serialize<TestRecord?>(null, _options));
    }

    [Fact]
    public void ShouldSerializeSomeLikeTheInnerType()
    {
        SerializeOption(Some(_record)).ShouldBe(JsonSerializer.Serialize(_record, _options));
    }

    [Fact]
    public void ShouldDeserializeNullTokenAsNone()
    {
        DeserializeOption(JsonSerializer.Serialize<TestRecord?>(null, _options)).ShouldBeEmpty();
    }

    [Fact]
    public void ShouldDeserializeObjectAsSome()
    {
        DeserializeOption(JsonSerializer.Serialize(_record, _options)).ShouldContain(_record);
    }
}
