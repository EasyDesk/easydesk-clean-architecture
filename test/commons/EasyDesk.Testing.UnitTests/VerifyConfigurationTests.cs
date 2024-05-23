using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using EasyDesk.Testing.VerifyConfiguration;
using Newtonsoft.Json;
using NodaTime;
using System.Runtime.CompilerServices;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.Testing.UnitTests;

public static class Setup
{
    [ModuleInitializer]
    public static void SetupTests()
    {
        VerifySettingsInitializer.Initialize();
    }
}

public class VerifyConfigurationTests
{
    [Fact]
    public Task VerifyOptionConversionTest()
    {
        return Verify(new
        {
            Some = Some(123),
            None,
            Nested = Some(Some(123)),
            NoneInt = (Option<int>)None,
        });
    }

    private record CustomError(int Field, Error Inner) : Error;

    private record CustomErrorInside(string AnotherField) : Error;

    [Fact]
    public Task VerifyErrorConversionTestCustom()
    {
        return Verify(new CustomError(42, new CustomErrorInside("hello")));
    }

    [Fact]
    public Task VerifyErrorConversionTestMultiEmpty()
    {
        return Verify(new MultiError(new CustomError(42, new CustomErrorInside("hello")), List<Error>()));
    }

    [Fact]
    public Task VerifyErrorConversionTestMulti()
    {
        return Verify(new MultiError(
                new CustomError(42, new CustomErrorInside("hello")),
                List<Error>(
                    new CustomErrorInside("asd"),
                    new CustomError(123, new CustomErrorInside("world")))));
    }

    [Fact]
    public Task EmptyCollectionConversionTest()
    {
        return Verify(new { Inner = new List<int>() });
    }

    [Fact]
    public Task NewtonsoftJsonTest()
    {
        return Verify(JsonConvert.DeserializeObject("{\"a\": [1, 2, 3]}"));
    }

    [Fact]
    public Task NodaTimeTest()
    {
        return Verify(new
        {
            LocalDate = new LocalDate(1994, 3, 1),
            LocalTime = new LocalTime(14, 25),
            Duration = Duration.FromHours(13),
        });
    }
}
