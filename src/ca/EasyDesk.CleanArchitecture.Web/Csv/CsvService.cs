using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using EasyDesk.CleanArchitecture.Web.Csv.Converters;
using EasyDesk.CleanArchitecture.Web.Formatting;
using EasyDesk.Commons.Results;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Web.Csv;

public class CsvService
{
    private readonly CsvConfiguration _configuration;
    private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

    public CsvService(CsvConfiguration configuration, IDateTimeZoneProvider dateTimeZoneProvider)
    {
        _configuration = configuration with
        {
            MissingFieldFound = args =>
            {
                var message = "does not exist.";

                // Get by index.
                if (args.HeaderNames is null || args.HeaderNames.Length == 0)
                {
                    throw new InvalidCsvLineException($"Field at index '{args.Index}'", message);
                }

                // Get by name.
                var indexText = args.Index > 0 ? $" at field index '{args.Index}'" : string.Empty;

                if (args.HeaderNames.Length == 1)
                {
                    throw new InvalidCsvLineException($"Field with name '{args.HeaderNames[0]}'{indexText}", message);
                }

                throw new InvalidCsvLineException($"Field containing names '{string.Join("' or '", args.HeaderNames)}'{indexText}", message);
            },
        };
        _dateTimeZoneProvider = dateTimeZoneProvider;
    }

    public IEnumerable<Result<T>> ParseCsv<T>(Stream content, Func<IReaderRow, T> converter, Action<CsvContext>? configureContext = null)
    {
        using var reader = new StreamReader(content);
        using var csv = new CsvReader(reader, _configuration);

        csv.Context.TypeConverterCache.AddConverterFactory(new CsvOptionConverterFactory());
        csv.Context.TypeConverterCache.AddConverter<AnnualDate>(new NodaTimeConverter<AnnualDate>(FormatDefaults.AnnualDate));
        csv.Context.TypeConverterCache.AddConverter<Duration>(new NodaTimeConverter<Duration>(FormatDefaults.Duration));
        csv.Context.TypeConverterCache.AddConverter<Instant>(new NodaTimeConverter<Instant>(FormatDefaults.Instant));
        csv.Context.TypeConverterCache.AddConverter<LocalDateTime>(new NodaTimeConverter<LocalDateTime>(FormatDefaults.LocalDateTime));
        csv.Context.TypeConverterCache.AddConverter<LocalDate>(new NodaTimeConverter<LocalDate>(FormatDefaults.LocalDate));
        csv.Context.TypeConverterCache.AddConverter<LocalTime>(new NodaTimeConverter<LocalTime>(FormatDefaults.LocalTime));
        csv.Context.TypeConverterCache.AddConverter<Offset>(new NodaTimeConverter<Offset>(FormatDefaults.Offset));
        csv.Context.TypeConverterCache.AddConverter<OffsetDateTime>(new NodaTimeConverter<OffsetDateTime>(FormatDefaults.OffsetDateTime));
        csv.Context.TypeConverterCache.AddConverter<OffsetDate>(new NodaTimeConverter<OffsetDate>(FormatDefaults.OffsetDate));
        csv.Context.TypeConverterCache.AddConverter<OffsetTime>(new NodaTimeConverter<OffsetTime>(FormatDefaults.OffsetTime));
        csv.Context.TypeConverterCache.AddConverter<Period>(new NodaTimeConverter<Period>(FormatDefaults.Period));
        csv.Context.TypeConverterCache.AddConverter<YearMonth>(new NodaTimeConverter<YearMonth>(FormatDefaults.YearMonth));
        csv.Context.TypeConverterCache.AddConverter<ZonedDateTime>(new NodaTimeConverter<ZonedDateTime>(FormatDefaults.ZonedDateTime.WithZoneProvider(_dateTimeZoneProvider)));
        configureContext?.Invoke(csv.Context);

        long lineIndex = 0;
        var result = Read(csv, lineIndex).FlatMap(more => more ? ReadHeader(csv, lineIndex) : false);
        lineIndex++;
        result = result.FlatMap(more => more ? Read(csv, lineIndex) : false);
        while (result.Contains(It))
        {
            yield return Convert(csv, lineIndex, converter);
            lineIndex++;
            result = Read(csv, lineIndex);
        }
        if (!result.IsFailure)
        {
            yield break;
        }

        yield return result.ReadError();
    }

    private Result<bool> Read(CsvReader csv, long lineIndex) =>
        CatchInvalidRecords(csv, lineIndex, csv => csv.Read());

    private Result<bool> ReadHeader(CsvReader csv, long lineIndex) =>
        CatchInvalidRecords(csv, lineIndex, csv => csv.ReadHeader());

    private Result<T> Convert<T>(CsvReader csv, long lineIndex, Func<IReaderRow, T> converter) =>
        CatchInvalidRecords(csv, lineIndex, converter);

    private Result<T> CatchInvalidRecords<T>(CsvReader csv, long lineIndex, Func<CsvReader, T> callable)
    {
        try
        {
            return callable(csv);
        }
        catch (BadDataException e)
        {
            return new InvalidCsvLine(lineIndex, e.Message, e.Field);
        }
        catch (TypeConverterException e)
        {
            return new InvalidCsvLine(lineIndex, CleanExceptionMessage(e.Message), e.Text);
        }
        catch (MissingCsvValueException e)
        {
            return new InvalidCsvLine(lineIndex, e.Message, e.Field);
        }
        catch (InvalidCsvLineException e)
        {
            return new InvalidCsvLine(lineIndex, e.Message, e.Field);
        }
        catch (ReaderException e)
        {
            return new InvalidCsvLine(lineIndex, CleanExceptionMessage(e.Message));
        }
    }

    private string CleanExceptionMessage(string dirty) => dirty.Split('\n', 2)[0].ReplaceLineEndings(string.Empty);

    public record InvalidCsvLine(long LineIndex, string Message, string? Field = null) : Error
    {
        public string DisplayMessage => $"Line {LineIndex}: {(Field is null ? string.Empty : Field + ' ')}{Message}";
    }
}
