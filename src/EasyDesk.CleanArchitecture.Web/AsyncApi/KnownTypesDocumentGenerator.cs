using System.Net.Mime;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Infrastructure.Json;
using EasyDesk.Tools.Reflection;
using Neuroglia;
using Neuroglia.AsyncApi;
using Neuroglia.AsyncApi.Configuration;
using Neuroglia.AsyncApi.Models;
using Neuroglia.AsyncApi.Services.FluentBuilders;
using Neuroglia.AsyncApi.Services.Generators;
using NJsonSchema;
using NJsonSchema.Generation;

namespace EasyDesk.CleanArchitecture.Web.AsyncApi;

public class KnownTypesDocumentGenerator : IAsyncApiDocumentGenerator
{
    public const string Version = "1.0.0";

    private readonly IAsyncApiDocumentBuilder _documentBuilder;
    private readonly KnownMessageTypes _knownMessageTypes;
    private readonly string _microserviceName;
    private readonly JsonSchemaGeneratorSettings _jsonSchemaGeneratorSettings;

    public KnownTypesDocumentGenerator(
        IAsyncApiDocumentBuilder documentBuilder,
        KnownMessageTypes knownMessageTypes,
        string microserviceName)
    {
        _documentBuilder = documentBuilder;
        _knownMessageTypes = knownMessageTypes;
        _microserviceName = microserviceName;
        _jsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings
        {
            SerializerSettings = JsonDefaults.DefaultSerializerSettings()
        };
    }

    public async Task<IEnumerable<AsyncApiDocument>> GenerateAsync(params Type[] markupTypes)
    {
        return await GenerateAsync(markupTypes, new AsyncApiDocumentGenerationOptions
        {
            AutomaticallyGenerateExamples = false
        });
    }

    public Task<IEnumerable<AsyncApiDocument>> GenerateAsync(IEnumerable<Type> markupTypes, AsyncApiDocumentGenerationOptions options)
    {
        return Task.FromResult(GenerateDocuments(_knownMessageTypes, options));
    }

    private IEnumerable<AsyncApiDocument> GenerateDocuments(KnownMessageTypes types, AsyncApiDocumentGenerationOptions options)
    {
        _documentBuilder
            .WithId(_microserviceName)
            .WithTitle(_microserviceName)
            .WithVersion(Version)
            .WithDefaultContentType(MediaTypeNames.Application.Json);

        foreach (var messageType in types.Types)
        {
            _documentBuilder.UseChannel(messageType.Name, channel =>
            {
                if (messageType.IsSubtypeOrImplementationOf(typeof(IIncomingCommand)))
                {
                    ConfigureOperation(channel, messageType, OperationType.Subscribe, "Command");
                }
                if (messageType.IsSubtypeOrImplementationOf(typeof(IOutgoingEvent)))
                {
                    ConfigureOperation(channel, messageType, OperationType.Publish, "Event");
                }
            });
        }
        options?.DefaultConfiguration?.Invoke(_documentBuilder);
        yield return _documentBuilder.Build();
    }

    private void ConfigureOperation(
        IChannelDefinitionBuilder builder,
        Type messageType,
        OperationType operationType,
        string messageClassifier)
    {
        builder.DefineOperation(operationType, operation =>
        {
            operation
                .WithOperationId(messageType.Name)
                .UseMessage(message => message
                    .WithName(messageType.Name)
                    .WithTitle(messageType.Name.ToCamelCase().SplitCamelCase())
                    .WithPayloadSchema(JsonSchema.FromType(messageType, _jsonSchemaGeneratorSettings)))
                .TagWith(tag => tag.WithName(messageClassifier));
        });
    }
}
