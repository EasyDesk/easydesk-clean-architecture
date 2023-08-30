using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Web.Csv;

public class CsvService
{
    private readonly CsvConfiguration _configuration;

    public CsvService(CsvConfiguration configuration)
    {
        _configuration = configuration with
        {
            MissingFieldFound = args =>
            {
                var message = "does not exist.";

                // Get by index.
                if (args.HeaderNames == null || args.HeaderNames.Length == 0)
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
    }

    public IEnumerable<Result<T>> ParseCsv<T>(Stream content, Func<IReaderRow, T> converter, Action<CsvContext>? configureContext = null)
    {
        using var reader = new StreamReader(content);
        using var csv = new CsvReader(reader, _configuration);

        configureContext?.Invoke(csv.Context);

        // Bug of CsvHelper: see https://github.com/JoshClose/CsvHelper/issues/2153.
        ////csv.Context.TypeConverterCache.AddConverterFactory(new CsvOptionConverterFactory());

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
        if (result.IsFailure)
        {
            yield return result.ReadError();
        }
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
