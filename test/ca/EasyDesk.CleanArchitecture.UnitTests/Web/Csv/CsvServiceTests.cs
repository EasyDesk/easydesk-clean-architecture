using CsvHelper;
using EasyDesk.CleanArchitecture.Web.Csv;
using System.Globalization;
using System.Text;

namespace EasyDesk.CleanArchitecture.UnitTests.Web.Csv;

[UsesVerify]
public class CsvServiceTests
{
    private readonly CsvService _sut = new(new(CultureInfo.InvariantCulture)
    {
        DetectDelimiter = true,
        PrepareHeaderForMatch = args => args.Header.Trim().ToLower(),
    });

    private IEnumerable<T> Parse<T>(string text, Func<IReaderRow, T> converter)
    {
        using var content = new MemoryStream(Encoding.UTF8.GetBytes(text));
        return _sut.ParseCsv(content, converter).ToList();
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

    [Fact(Skip = "https://github.com/JoshClose/CsvHelper/issues/2153")]
    public async Task ShouldParseRecordsAutomatically()
    {
        var csv = """
            String;Integer
            A;1
            ;2
            C;
            """;
        await Verify(Parse(csv, row => row.GetRecord(new
        {
            String = default(Option<string>),
            Integer = default(Option<int>),
        })));
    }
}
