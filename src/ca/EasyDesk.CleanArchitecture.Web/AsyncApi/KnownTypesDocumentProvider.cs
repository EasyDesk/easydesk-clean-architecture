using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Web.Versioning;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using Saunter.Generation;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Web.AsyncApi;

internal class KnownTypesDocumentProvider : IAsyncApiDocumentProvider
{
    private readonly IDocumentGenerator _generator;
    private readonly RebusMessagingOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public KnownTypesDocumentProvider(
        IDocumentGenerator generator,
        RebusMessagingOptions options,
        IServiceProvider serviceProvider)
    {
        _generator = generator;
        _options = options;
        _serviceProvider = serviceProvider;
    }

    public AsyncApiDocument GetDocument(AsyncApiOptions options, AsyncApiDocument prototype)
    {
        var documentVersion = prototype.Info.Version;
        var types = _options
            .KnownMessageTypes
            .Where(t => t.GetApiVersionFromNamespace().MapToString().All(x => x == documentVersion))
            .Select(t => t.GetTypeInfo())
            .ToArray();
        return _generator.GenerateDocument(types, options, prototype, _serviceProvider);
    }
}
