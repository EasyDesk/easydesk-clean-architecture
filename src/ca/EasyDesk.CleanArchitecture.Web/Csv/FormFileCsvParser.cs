using CsvHelper;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Results;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using static EasyDesk.CleanArchitecture.Web.Csv.CsvService;

namespace EasyDesk.CleanArchitecture.Web.Csv;

public class FormFileCsvParser
{
    public const long DefaultMaxUploadSize = 1024 * 1024 * 4;

    private readonly CsvService _csvService;

    public FormFileCsvParser(CsvService csvService)
    {
        _csvService = csvService;
    }

    private Result<IEnumerable<Result<T>>> InnerLazyParseFormFileAsCsv<T>(
        IFormFile formFile,
        Func<IReaderRow, T> converter,
        long maxSize = DefaultMaxUploadSize,
        Action<CsvContext>? configureContext = null,
        string? propertyName = null)
    {
        propertyName ??= formFile.FileName;
        if (!formFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return Errors.InvalidInput(propertyName, "Csv.InvalidExtension", "File is not a CSV.");
        }
        if (formFile.Length > DefaultMaxUploadSize)
        {
            return Errors.InvalidInput(propertyName, "Csv.FileTooLarge", $"File exceeds maximum upload size of {maxSize / 1024}kB.");
        }
        var contentType = new ContentType(formFile.ContentType);
        if (contentType.MediaType != "text/csv")
        {
            return Errors.InvalidInput(propertyName, "Csv.InvalidContentType", $"File should be in CSV format and have text/csv as content type.");
        }
        return Success(Parse(formFile, converter, configureContext).Select(r => r.MapError(e => e switch
        {
            InvalidCsvLine x => Errors.InvalidInput(propertyName, "Csv.InvalidFormat", x.DisplayMessage),
            _ => e
        })).EnumerateOnce());
    }

    public Result<IEnumerable<Result<T>>> LazyParseFormFileAsCsv<T>(
        IFormFile formFile,
        Func<IReaderRow, T> converter,
        long maxSize = DefaultMaxUploadSize,
        Action<CsvContext>? configureContext = null,
        [CallerArgumentExpression(nameof(formFile))] string? propertyName = null) =>
            InnerLazyParseFormFileAsCsv(formFile, converter, maxSize, configureContext, propertyName);

    public Result<IEnumerable<T>> EagerParseFormFileAsCsv<T>(
        IFormFile formFile,
        Func<IReaderRow, T> converter,
        long maxSize = DefaultMaxUploadSize,
        Action<CsvContext>? configureContext = null,
        [CallerArgumentExpression(nameof(formFile))] string? propertyName = null) =>
            InnerLazyParseFormFileAsCsv(formFile, converter, maxSize, configureContext, propertyName)
                .FlatMap(StaticImports.CatchAllFailures);

    public Result<IEnumerable<T>> GreedyParseFormFileAsCsv<T>(
        IFormFile formFile,
        Func<IReaderRow, T> converter,
        long maxSize = DefaultMaxUploadSize,
        Action<CsvContext>? configureContext = null,
        [CallerArgumentExpression(nameof(formFile))] string? propertyName = null) =>
            InnerLazyParseFormFileAsCsv(formFile, converter, maxSize, configureContext, propertyName)
                .FlatMap(StaticImports.CatchFirstFailure);

    private IEnumerable<Result<T>> Parse<T>(IFormFile formFile, Func<IReaderRow, T> converter, Action<CsvContext>? configureContext)
    {
        using var stream = formFile.OpenReadStream();
        foreach (var record in _csvService.ParseCsv(stream, converter, configureContext))
        {
            yield return record;
        }
    }
}
