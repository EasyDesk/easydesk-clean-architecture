using EasyDesk.CleanArchitecture.Infrastructure.Json.Converters;
using EasyDesk.Tools.Options;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Json.Converters;

public class OptionConverterTests
{
    private record TestRecord(int Value);

    private readonly OptionConverter _sut = new();
    private readonly TestRecord _record = new(10);

    private string SerializeOption(Option<TestRecord> value) => JsonConvert.SerializeObject(value, _sut);

    private Option<TestRecord> DeserializeOption(string json) => JsonConvert.DeserializeObject<Option<TestRecord>>(json, _sut);

    [Fact]
    public void ShouldSerializeNoneAsANullToken()
    {
        SerializeOption(None).ShouldBe(JsonConvert.Null);
    }

    [Fact]
    public void ShouldSerializeSomeLikeTheInnerType()
    {
        SerializeOption(Some(_record)).ShouldBe(JsonConvert.SerializeObject(_record));
    }

    [Fact]
    public void ShouldDeserializeNullTokenAsNone()
    {
        DeserializeOption(JsonConvert.Null).ShouldBeEmpty();
    }

    [Fact]
    public void ShouldDeserializeObjectAsSome()
    {
        DeserializeOption(JsonConvert.SerializeObject(_record)).ShouldContain(_record);
    }
}
