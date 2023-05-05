using CsvHelper;
using CsvHelper.Configuration;

namespace EasyDesk.CleanArchitecture.Web.Csv;

public class CsvService
{
    private readonly CsvConfiguration _configuration;

    public CsvService(CsvConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IEnumerable<T> ParseCsv<T>(Stream content, Func<IReaderRow, T> converter, Action<CsvContext>? configureContext = null)
    {
        using var reader = new StreamReader(content);
        using var csv = new CsvReader(reader, _configuration);

        // Bug of CsvHelper: see https://github.com/JoshClose/CsvHelper/issues/2153.
        ////csv.Context.TypeConverterCache.AddConverterFactory(new CsvOptionConverterFactory());
        configureContext?.Invoke(csv.Context);
        csv.Read();
        csv.ReadHeader();
        while (csv.Read())
        {
            yield return converter(csv);
        }
    }
}
