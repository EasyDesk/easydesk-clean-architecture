using EasyDesk.CleanArchitecture.Application.Messaging;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using Saunter.Generation;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Web.AsyncApi;

internal class KnowTypesDocumentProvider : IAsyncApiDocumentProvider
{
    private readonly IDocumentGenerator _documentGenerator;
    private readonly KnownMessageTypes _knownMessageTypes;
    private readonly IServiceProvider _serviceProvider;

    public KnowTypesDocumentProvider(
        IDocumentGenerator documentGenerator,
        KnownMessageTypes knownMessageTypes,
        IServiceProvider serviceProvider)
    {
        _documentGenerator = documentGenerator;
        _knownMessageTypes = knownMessageTypes;
        _serviceProvider = serviceProvider;
    }

    public AsyncApiDocument GetDocument(AsyncApiOptions options, AsyncApiDocument prototype) =>
        _documentGenerator.GenerateDocument(_knownMessageTypes.Types.Select(t => t.GetTypeInfo()).ToArray(), options, prototype, _serviceProvider);
}
