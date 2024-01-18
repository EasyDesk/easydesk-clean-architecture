using CsvHelper;
using EasyDesk.CleanArchitecture.Web.Csv;
using EasyDesk.Commons.Results;
using NSubstitute;
using System.Globalization;
using System.Text;

namespace EasyDesk.CleanArchitecture.UnitTests.Web.Csv;

public class CsvServiceTests
{
    private readonly CsvService _sut = new(new(CultureInfo.InvariantCulture)
    {
        DetectDelimiter = true,
        PrepareHeaderForMatch = args => args.Header.Trim().ToLower(),
    });

    private IEnumerable<T> Parse<T>(string text, Func<IReaderRow, T> converter) =>
        SafeParse(text, converter).Select(r => r.ThrowIfFailure());

    private IEnumerable<Result<T>> SafeParse<T>(string text, Func<IReaderRow, T> converter)
    {
        using var content = new MemoryStream(Encoding.UTF8.GetBytes(text));
        foreach (var e in _sut.ParseCsv(content, converter))
        {
            yield return e;
        }
    }

    [Fact]
    public async Task ShouldParseRequiredFields()
    {
        var csv = """
            String;Integer
            A;1
            B;2
            C;3
            """;
        await Verify(Parse(csv, row => new
        {
            String = row.GetRequiredField<string>("String"),
            Integer = row.GetRequiredField<int>("Integer"),
        }));
    }

    [Fact]
    public async Task ShouldParseOptionalFields()
    {
        var csv = """
            String;Integer
            A;1
            ;2
            C;
            """;
        await Verify(Parse(csv, row => new
        {
            String = row.GetOptionalField<string>("String"),
            Integer = row.GetOptionalField<int>("Integer"),
        }));
    }

    [Fact]
    public async Task ShouldBeLazy()
    {
        var csv = """
            String;Integer
            A;1
            B;2
            C;3
            ';"kek";;;;;;;;;;;;;;;;;;;;;;XD;;;;;;;;;;
            """;
        var counter = 0;
        await Verify(Parse(csv, row => ++counter == 4 ? throw new Exception("The parser isn't lazy") : new
        {
            String = row.GetOptionalField<string>("String"),
            Integer = row.GetOptionalField<int>("Integer"),
        }).Take(3));
    }

    [Fact]
    public void ShouldCallConverter_ForEachDataRow_WhenConsumed()
    {
        var csv = """
            String;Integer
            A;1
            B;2
            C;3
            """;
        var converter = Substitute.For<Func<IReaderRow, object>>();
        var parsed = Parse(csv, converter);
        converter.ReceivedWithAnyArgs(0).Invoke(null!);
        var consumed = parsed.All(_ => true);
        converter.ReceivedWithAnyArgs(3).Invoke(null!);
        consumed = parsed.All(_ => true);
        converter.ReceivedWithAnyArgs(6).Invoke(null!);
    }

    [Fact]
    public async Task ShouldReturnError_IfCsvIsMalformed()
    {
        var csv = """
            String;String
            A;1
            ;2
            C;
            """;
        await Verify(SafeParse(csv, row => new
        {
            String = row.GetOptionalField<string>("String"),
            Integer = row.GetOptionalField<int>("String"),
        }));
    }

    [Fact]
    public async Task ShouldFail_IfRequiredColumnIsMissing()
    {
        var csv = """
            String;Integer
            A;1
            B;2
            """;
        await Verify(SafeParse(csv, row => new
        {
            X = row.GetRequiredField<bool>("Invalid"),
        }));
    }

    ////[Fact]
    ////public async Task ShouldParseRecordsAutomatically()
    ////{
    ////    var csv = """
    ////        String;Integer
    ////        A;1
    ////        ;2
    ////        C;
    ////        """;
    ////    await Verify(Parse(csv, row => row.GetRecord(new
    ////    {
    ////        String = default(Option<string>),
    ////        Integer = default(Option<int>),
    ////    })));
    ////}
}
