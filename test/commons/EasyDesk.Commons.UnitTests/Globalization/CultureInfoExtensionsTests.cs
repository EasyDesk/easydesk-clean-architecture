using EasyDesk.Commons.Globalization;
using Shouldly;
using System.Globalization;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Globalization;

public class CultureInfoExtensionsTests
{
    public static IEnumerable<object[]> UseCases()
    {
        yield return new object[] { new CultureInfo("en-US"), };
        yield return new object[] { new CultureInfo("it-IT"), };
    }

    [Theory]
    [MemberData(nameof(UseCases))]
    public void Use_ShouldChangeCurrentCulture(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        culture.Use(() => CultureInfo.CurrentCulture.ShouldBe(culture));
    }

    [Theory]
    [MemberData(nameof(UseCases))]
    public void Use_ShouldRestoreCurrentCulture(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = culture;

        CultureInfo.InvariantCulture.Use(() => CultureInfo.CurrentCulture.ShouldBe(CultureInfo.InvariantCulture));

        CultureInfo.CurrentCulture.ShouldBe(culture);
    }

    [Theory]
    [MemberData(nameof(UseCases))]
    public void Use_ShouldChangeCurrentCulture_AndReturnResult(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        var result = culture.Use(() => CultureInfo.CurrentCulture);

        result.ShouldBe(culture);
    }

    [Theory]
    [MemberData(nameof(UseCases))]
    public void Use_ShouldRestoreCurrentCulture_AfterReturningResult(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = culture;

        var result = CultureInfo.InvariantCulture.Use(() => CultureInfo.CurrentCulture);

        result.ShouldBe(CultureInfo.InvariantCulture);

        CultureInfo.CurrentCulture.ShouldBe(culture);
    }

    [Theory]
    [MemberData(nameof(UseCases))]
    public async Task UseAsync_ShouldChangeCurrentCulture(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        await culture.UseAsync(() =>
        {
            CultureInfo.CurrentCulture.ShouldBe(culture);
            return Task.CompletedTask;
        });
    }

    [Theory]
    [MemberData(nameof(UseCases))]
    public async Task UseAsync_ShouldRestoreCurrentCulture(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = culture;

        await CultureInfo.InvariantCulture.UseAsync(() =>
        {
            CultureInfo.CurrentCulture.ShouldBe(CultureInfo.InvariantCulture);
            return Task.CompletedTask;
        });

        CultureInfo.CurrentCulture.ShouldBe(culture);
    }

    [Theory]
    [MemberData(nameof(UseCases))]
    public async Task UseAsync_ShouldChangeCurrentCulture_AndReturnResult(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        var result = await culture.UseAsync(() => Task.FromResult(CultureInfo.CurrentCulture));

        result.ShouldBe(culture);
    }

    [Theory]
    [MemberData(nameof(UseCases))]
    public async Task UseAsync_ShouldRestoreCurrentCulture_AfterReturningResult(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = culture;

        var result = await CultureInfo.InvariantCulture.UseAsync(() => Task.FromResult(CultureInfo.CurrentCulture));

        result.ShouldBe(CultureInfo.InvariantCulture);

        CultureInfo.CurrentCulture.ShouldBe(culture);
    }
}
