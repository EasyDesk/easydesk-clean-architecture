using CsvHelper;
using EasyDesk.CleanArchitecture.Web.Csv;
using EasyDesk.CleanArchitecture.Web.Formatting;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using NodaTime;
using NSubstitute;
using System.Globalization;
using System.Text;

namespace EasyDesk.CleanArchitecture.UnitTests.Web.Csv;

public class CsvServiceTests
{
    private readonly CsvService _sut = new(
        new(CultureInfo.InvariantCulture)
        {
            DetectDelimiter = true,
            PrepareHeaderForMatch = args => args.Header.Trim().ToLower(),
        },
        DateTimeZoneProviders.Tzdb);

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
            String;Integer;LocalTime
            A;1;10:00:00
            ;2;
            C;;
            """;
        await Verify(Parse(csv, row => new
        {
            String = row.GetField<Option<string>>("String"),
            Integer = row.GetField<Option<int>>("Integer"),
            LocalTime = row.GetField<Option<LocalTime>>("LocalTime"),
        }));
    }

    [Fact]
    public async Task ShouldParseNodaTimeFields()
    {
        var separator = ',';
        var csv = new StringBuilder()
            .AppendLine("AnnualDate;Duration;Instant;LocalDateTime;LocalDate;LocalTime;Offset;OffsetDateTime;OffsetDate;OffsetTime;Period;YearMonth;ZonedDateTime".Replace(';', separator))
            .Append(FormatDefaults.AnnualDate.Format(new AnnualDate()))
            .Append(separator)
            .Append(FormatDefaults.Duration.Format(Duration.Zero))
            .Append(separator)
            .Append(FormatDefaults.Instant.Format(new Instant()))
            .Append(separator)
            .Append(FormatDefaults.LocalDateTime.Format(new LocalDateTime()))
            .Append(separator)
            .Append(FormatDefaults.LocalDate.Format(new LocalDate()))
            .Append(separator)
            .Append(FormatDefaults.LocalTime.Format(new LocalTime()))
            .Append(separator)
            .Append(FormatDefaults.Offset.Format(Offset.FromHours(1)))
            .Append(separator)
            .Append(FormatDefaults.OffsetDateTime.Format(new OffsetDateTime()))
            .Append(separator)
            .Append(FormatDefaults.OffsetDate.Format(new OffsetDate()))
            .Append(separator)
            .Append(FormatDefaults.OffsetTime.Format(new OffsetTime()))
            .Append(separator)
            .Append(FormatDefaults.Period.Format(Period.FromDays(1)))
            .Append(separator)
            .Append(FormatDefaults.YearMonth.Format(new YearMonth()))
            .Append(separator)
            .Append(FormatDefaults.ZonedDateTime.Format(new ZonedDateTime()))
            .ToString();
        await Verify(Parse(csv, row => new
        {
            AnnualDate = row.GetField<Option<AnnualDate>>("AnnualDate"),
            Duration = row.GetField<Option<Duration>>("Duration"),
            Instant = row.GetField<Option<Instant>>("Instant"),
            LocalDateTime = row.GetField<Option<LocalDateTime>>("LocalDateTime"),
            LocalDate = row.GetField<Option<LocalDate>>("LocalDate"),
            LocalTime = row.GetField<Option<LocalTime>>("LocalTime"),
            Offset = row.GetField<Option<Offset>>("Offset"),
            OffsetDateTime = row.GetField<Option<OffsetDateTime>>("OffsetDateTime"),
            OffsetDate = row.GetField<Option<OffsetDate>>("OffsetDate"),
            OffsetTime = row.GetField<Option<OffsetTime>>("OffsetTime"),
            Period = row.GetField<Option<Period>>("Period"),
            YearMonth = row.GetField<Option<YearMonth>>("YearMonth"),
            ZonedDateTime = row.GetField<Option<ZonedDateTime>>("ZonedDateTime"),
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
        await Verify(
            Parse(csv, row => ++counter == 4 ? throw new InvalidOperationException("The parser isn't lazy") : new
            {
                String = row.GetField<Option<string>>("String"),
                Integer = row.GetField<Option<int>>("Integer"),
            })
                .Take(3));
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
            String = row.GetField<Option<string>>("String"),
            Integer = row.GetField<Option<int>>("String"),
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

    [Fact]
    public async Task ShouldFail_IfRequiredValueIsMissing()
    {
        var csv = """
            String;Integer
            A;1
            B;
            """;
        await Verify(SafeParse(csv, row => new
        {
            X = row.GetRequiredField<int>("Integer"),
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
