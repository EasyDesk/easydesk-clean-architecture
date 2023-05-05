using CsvHelper;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;

namespace EasyDesk.CleanArchitecture.Web.Csv;

public class FormFileCsvParser
{
    public const long DefaultMaxUploadSize = 1024 * 1024 * 4;

    private readonly CsvService _csvService;

    public FormFileCsvParser(CsvService csvService)
    {
        _csvService = csvService;
    }

    public Result<IEnumerable<T>> ParseFormFileAsCsv<T>(
        IFormFile formFile,
        Func<IReaderRow, T> converter,
        long maxSize = DefaultMaxUploadSize,
        Action<CsvContext>? configureContext = null,
        [CallerArgumentExpression(nameof(formFile))] string? propertyName = null)
    {
        propertyName ??= formFile.FileName;
        if (!formFile.FileName.EndsWith(".csv"))
        {
            return Errors.InvalidInput(propertyName, "File is not a CSV.");
        }
        if (formFile.Length > DefaultMaxUploadSize)
        {
            return Errors.InvalidInput(propertyName, $"File exceeds maximum upload size of {maxSize / 1024}kB.");
        }
        if (formFile.ContentType != "text/csv" || !formFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return Errors.InvalidInput(propertyName, $"File should be in CSV format and have text/csv as content type.");
        }
        return Success(Parse(formFile, converter, configureContext).EnumerateOnce());
    }

    private IEnumerable<T> Parse<T>(IFormFile formFile, Func<IReaderRow, T> converter, Action<CsvContext>? configureContext)
    {
        using var stream = formFile.OpenReadStream();
        foreach (var record in _csvService.ParseCsv(stream, converter, configureContext))
        {
            yield return record;
        }
    }
}
