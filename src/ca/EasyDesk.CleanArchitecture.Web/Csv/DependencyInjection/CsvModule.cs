using Autofac;
using CsvHelper;
using CsvHelper.Configuration;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Web.Csv.DependencyInjection;

public partial class CsvModule : AppModule
{
    private readonly CsvConfiguration _configuration;

    public CsvModule(CsvConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterInstance(_configuration)
            .SingleInstance();

        builder.RegisterType<CsvService>()
            .SingleInstance();

        builder.RegisterType<FormFileCsvParser>()
            .SingleInstance();
    }

    public static string DefaultPrepareHeaderForMatch(PrepareHeaderForMatchArgs args) =>
        WhitespaceRegex().Replace(args.Header, string.Empty).ToLower();

    [GeneratedRegex(@"\s")]
    private static partial Regex WhitespaceRegex();
}

public static class CsvModuleExtensions
{
    public static IAppBuilder AddCsvParsing(this IAppBuilder builder, CultureInfo? cultureInfo = null, Action<CsvConfiguration>? configure = null)
    {
        var configuration = new CsvConfiguration(cultureInfo ?? CultureInfo.InvariantCulture)
        {
            DetectDelimiter = true,
            PrepareHeaderForMatch = CsvModule.DefaultPrepareHeaderForMatch,
            DetectColumnCountChanges = true,
        };
        configure?.Invoke(configuration);
        return builder.AddModule(new CsvModule(configuration));
    }
}
