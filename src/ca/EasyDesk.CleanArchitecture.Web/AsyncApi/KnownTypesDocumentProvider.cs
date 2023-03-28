using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using Saunter.Generation;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Web.AsyncApi;

internal class KnownTypesDocumentProvider : IAsyncApiDocumentProvider
{
    private readonly IDocumentGenerator _documentGenerator;
    private readonly RebusMessagingOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public KnownTypesDocumentProvider(
        IDocumentGenerator documentGenerator,
        RebusMessagingOptions options,
        IServiceProvider serviceProvider)
    {
        _documentGenerator = documentGenerator;
        _options = options;
        _serviceProvider = serviceProvider;
    }

    public AsyncApiDocument GetDocument(AsyncApiOptions options, AsyncApiDocument prototype) =>
        _documentGenerator.GenerateDocument(_options.KnownMessageTypes.Select(t => t.GetTypeInfo()).ToArray(), options, prototype, _serviceProvider);
}
