using CsvHelper;
using CsvHelper.Configuration;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Web.Csv.DependencyInjection;

public partial class CsvModule : AppModule
{
    private readonly CultureInfo _cultureInfo;
    private readonly Action<CsvConfiguration>? _configure;

    public CsvModule(CultureInfo cultureInfo, Action<CsvConfiguration>? configure = null)
    {
        _cultureInfo = cultureInfo;
        _configure = configure;
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        var configuration = new CsvConfiguration(_cultureInfo)
        {
            DetectDelimiter = true,
            PrepareHeaderForMatch = DefaultPrepareHeaderForMatch,
            DetectColumnCountChanges = true,
        };
        _configure?.Invoke(configuration);
        services.AddSingleton(configuration);
        services.AddSingleton<CsvService>();
        services.AddSingleton<FormFileCsvParser>();
    }

    public static string DefaultPrepareHeaderForMatch(PrepareHeaderForMatchArgs args) =>
        WhitespaceRegex().Replace(args.Header, string.Empty).ToLower();

    [GeneratedRegex(@"\s")]
    private static partial Regex WhitespaceRegex();
}

public static class CsvModuleExtensions
{
    public static AppBuilder AddCsvParsing(this AppBuilder builder, CultureInfo? cultureInfo = null, Action<CsvConfiguration>? configure = null)
    {
        return builder.AddModule(new CsvModule(cultureInfo ?? CultureInfo.InvariantCulture, configure));
    }
}
