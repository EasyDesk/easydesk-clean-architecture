using CsvHelper.Configuration;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace EasyDesk.CleanArchitecture.Web.Csv.DependencyInjection;

public class CsvModule : AppModule
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
            PrepareHeaderForMatch = args => args.Header.Trim().ToLower(),
            DetectColumnCountChanges = true,
        };
        _configure?.Invoke(configuration);
        services.AddSingleton(configuration);
        services.AddSingleton<CsvService>();
        services.AddSingleton<FormFileCsvParser>();
    }
}

public static class CsvModuleExtensions
{
    public static AppBuilder AddCsvParsing(this AppBuilder builder, CultureInfo? cultureInfo = null, Action<CsvConfiguration>? configure = null)
    {
        return builder.AddModule(new CsvModule(cultureInfo ?? CultureInfo.InvariantCulture, configure));
    }
}
