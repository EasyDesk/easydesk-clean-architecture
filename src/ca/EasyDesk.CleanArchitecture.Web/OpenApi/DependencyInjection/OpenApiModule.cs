using Autofac;
using EasyDesk.CleanArchitecture.Application.CommandLine;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerUI;
using CommandLine = System.CommandLine;

namespace EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;

public class OpenApiModule : AppModule
{
    public const string SingleVersionedDocumentName = "main";

    public OpenApiModuleOptions Options { get; }

    public OpenApiModule(Action<OpenApiModuleOptions>? configure)
    {
        var options = new OpenApiModuleOptions();
        configure?.Invoke(options);
        Options = options;
    }

    protected override void ConfigureServices(AppDescription app, IServiceCollection services)
    {
        var documentNames = app.GetModule<ApiVersioningModule>().Match(
            some: module => module.ApiVersioningInfo!.SupportedVersions.Select(VersionToDocumentName),
            none: () => [SingleVersionedDocumentName,]);
        foreach (var documentName in documentNames)
        {
            services.AddOpenApi(documentName);
        }
        services.AddCliCommand("openapi", (b, c) => OpenApiCommand(b, c.GetRequiredService<IComponentContext>(), documentNames));
        services.AddTransient<IConfigureOptions<OpenApiOptions>, OpenApiGeneratorOptionsConfigurer>();
        services.Configure<SwaggerUIOptions>(c => c.DocumentTitle = $"{app.Name} - Swagger");
    }

    public static string VersionToDocumentName(ApiVersion v) => v.ToString();

    public static Commons.Options.Option<ApiVersion> DocumentNameToVersion(string documentName) => Some(documentName)
        .Filter(d => d != SingleVersionedDocumentName)
        .Map(d => ApiVersion.Parse(d).OrElseThrow(() => throw new InvalidOperationException("Unable to retrieve the Api version from the document name.")));

    private void OpenApiCommand(CliCommandBuilder builder, IComponentContext componentContext, IEnumerable<string> documentNames)
    {
        var hostOption = new CommandLine.Option<string?>("--host")
        {
            Description = "Sets the host value in the OpenApi document",
        };
        var basePathOption = new CommandLine.Option<string?>("--base-path")
        {
            Description = "Sets the basePath value in the OpenApi document",
        };
        var defaultDocumentKey = componentContext.ResolveOption<ApiVersioningInfo>().Match(
            some: info => info.SupportedVersions.MaxOption().Map(VersionToDocumentName),
            none: () => Some(SingleVersionedDocumentName));
        var openApiVersionOption = new CommandLine.Option<OpenApiSpecVersion?>("--version")
        {
            Description = "The OpenApi version to use",
        };
        var documentNameOption = new CommandLine.Option<string>("--document")
        {
            CustomParser = result =>
            {
                if (result.Tokens.Count == 0)
                {
                    if (defaultDocumentKey)
                    {
                        return defaultDocumentKey.Value;
                    }
                    result.AddError("Default document not available");
                    return string.Empty;
                }
                var value = result.Tokens.Single().Value;
                if (!documentNames.Contains(value))
                {
                    result.AddError($"The document '{value}' does not exist");
                    return string.Empty;
                }
                return value;
            },
            Required = false,
            Description = "The name of the document to generate",
            DefaultValueFactory = arg => defaultDocumentKey.OrElseGet(() =>
            {
                arg.AddError("Default document not available");
                return string.Empty;
            }),
        };
        var formatJsonOption = new CommandLine.Option<bool>("--json")
        {
            Description = "True if the format of the output should be Json instead of Yaml",
            DefaultValueFactory = _ => false,
        };
        builder
            .AddOption(documentNameOption)
            .AddOption(hostOption)
            .AddOption(basePathOption)
            .AddOption(formatJsonOption)
            .AddOption(documentNameOption)
            .HandleWith(async context =>
            {
                var openapiProvider = componentContext.ResolveKeyed<IOpenApiDocumentProvider>(context.ParseResult.GetValue(documentNameOption) ?? string.Empty);

                var doc = await openapiProvider.GetOpenApiDocumentAsync(context.CancellationToken);

                var stream = Console.OpenStandardOutput();
                await doc.SerializeAsync(
                    stream: stream,
                    specVersion: context.ParseResult.GetValue(openApiVersionOption) ?? OpenApiSpecVersion.OpenApi3_1,
                    format: context.ParseResult.GetValue(formatJsonOption) ? OpenApiConstants.Json : OpenApiConstants.Yaml,
                    cancellationToken: context.CancellationToken);
            });
    }
}

public static class OpenApiModuleExtensions
{
    public const string DefaultRoutePrefix = "openapi";
    public const string DefaultEndpointTemplate = "swagger/{documentName}/swagger.json";

    public static IAppBuilder AddOpenApi(this IAppBuilder builder, Action<OpenApiModuleOptions>? configure = null)
    {
        return builder.AddModule(new OpenApiModule(configure));
    }

    public static bool HasOpenApi(this AppDescription app) => app.HasModule<OpenApiModule>();

    public static void UseOpenApiModule(this WebApplication app)
    {
        var openApiEndpoints = app.Services.GetOpenApiEndpoints();

        app.MapOpenApi($"{DefaultRoutePrefix}/{DefaultEndpointTemplate}");
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = DefaultRoutePrefix;
            openApiEndpoints.ForEach(endpoint =>
            {
                c.SwaggerEndpoint(endpoint.Path, endpoint.Key);
            });
        });
    }

    public static IEnumerable<(string Key, string Path)> GetOpenApiEndpoints(this IServiceProvider serviceProvider) => serviceProvider
        .GetServiceAsOption<ApiVersioningInfo>()
        .Map(info => info.SupportedVersions.OrderDescending().Select(OpenApiModule.VersionToDocumentName))
        .OrElse([OpenApiModule.SingleVersionedDocumentName,])
        .Select(documentName => (documentName, $"/{DefaultRoutePrefix}/{DefaultEndpointTemplate}".Replace("{documentName}", documentName)));
}

public sealed class OpenApiModuleOptions
{
    public Action<OpenApiOptions>? ConfigureOpenApi { get; set; }
}
