using System.Net.Mime;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.Tools.Reflection;
using Neuroglia;
using Neuroglia.AsyncApi;
using Neuroglia.AsyncApi.Configuration;
using Neuroglia.AsyncApi.Models;
using Neuroglia.AsyncApi.Services.FluentBuilders;
using Neuroglia.AsyncApi.Services.Generators;
using NJsonSchema;
using NJsonSchema.Generation;
using Rebus.Topic;

namespace EasyDesk.CleanArchitecture.Web.Neuroglia;
public class AsyncApiDocumentGenerator : IAsyncApiDocumentGenerator
{
    private readonly IAsyncApiDocumentBuilder _documentBuilder;
    private readonly KnownMessageTypes _knownMessageTypes;
    private readonly string _microserviceName;
    private readonly ITopicNameConvention _topicNameConvention;

    public AsyncApiDocumentGenerator(
        IAsyncApiDocumentBuilder documentBuilder,
        KnownMessageTypes knownMessageTypes,
        string microserviceName,
        ITopicNameConvention topicNameConvention)
    {
        _documentBuilder = documentBuilder;
        _knownMessageTypes = knownMessageTypes;
        _microserviceName = microserviceName;
        _topicNameConvention = topicNameConvention;
    }

    public Task<IEnumerable<AsyncApiDocument>> GenerateAsync(params Type[] markupTypes)
    {
        return GenerateAsync(markupTypes, null);
    }

    public virtual Task<IEnumerable<AsyncApiDocument>> GenerateAsync(IEnumerable<Type> markupTypes, AsyncApiDocumentGenerationOptions options)
    {
        return Task.FromResult(GenerateDocuments(_knownMessageTypes, options));
    }

    private IEnumerable<AsyncApiDocument> GenerateDocuments(KnownMessageTypes types, AsyncApiDocumentGenerationOptions options)
    {
        _documentBuilder
            .WithId(_microserviceName)
            .WithTitle($"{_microserviceName} AsyncApi")
            .WithDefaultContentType(MediaTypeNames.Application.Json);

        // TODO: use rebus to fetch information about the servers (message brokers)
        foreach (var messageType in types.Types)
        {
            _documentBuilder.UseChannel(messageType.Name, channel =>
            {
                if (messageType.IsSubtypeOrImplementationOf(typeof(IIncomingCommand)))
                {
                    ConfigureChannelForIncomingCommand(channel, messageType);
                }
                if (messageType.IsSubtypeOrImplementationOf(typeof(IOutgoingCommand)))
                {
                    ConfigureChannelForOutgoingCommand(channel, messageType);
                }
                if (messageType.IsSubtypeOrImplementationOf(typeof(IIncomingEvent)))
                {
                    ConfigureChannelForIncomingEvent(channel, messageType, options);
                }
                if (messageType.IsSubtypeOrImplementationOf(typeof(IOutgoingEvent)))
                {
                    ConfigureChannelForOutgoingEvent(channel, messageType, options);
                }
            });
        }
        options?.DefaultConfiguration?.Invoke(_documentBuilder);
        yield return _documentBuilder.Build();
    }

    private void ConfigureChannelForIncomingEvent(IChannelDefinitionBuilder builder, Type messageType, AsyncApiDocumentGenerationOptions options)
    {
        builder.DefineSubscribeOperation(setup =>
        {
            ConfigureChannelForOperation(messageType, options, setup, "Subscribe");
        });
    }

    private void ConfigureChannelForOutgoingEvent(IChannelDefinitionBuilder builder, Type messageType, AsyncApiDocumentGenerationOptions options)
    {
        builder.DefinePublishOperation(setup =>
        {
            ConfigureChannelForOperation(messageType, options, setup, "Publish");
        });
    }

    private static void ConfigureChannelForOperation(Type messageType, AsyncApiDocumentGenerationOptions options, IOperationDefinitionBuilder setup, string operationName)
    {
        // TODO: use topic name convention?
        setup
            .WithOperationId($"{messageType.Name}.{operationName}")
            .UseMessage(message =>
            {
                var settings = new JsonSchemaGeneratorSettings();
                //// TODO: use custom settings
                var messageSchema = JsonSchema.FromType(messageType, settings);
                message
                    .WithName(messageType.Name)
                    .WithTitle(messageType.Name.ToCamelCase().SplitCamelCase())
                    .WithPayloadSchema(messageSchema);

                if (options.AutomaticallyGenerateExamples)
                {
                    foreach (var example in messageSchema.GenerateExamples())
                    {
                        message.AddExample(example.Key, example.Value);
                    }
                }
            })
            .TagWith(tag =>
            {
                tag
                    .WithName(operationName)
                    .WithDescription($"{operationName} operation");
            });
    }

    private void ConfigureChannelForIncomingCommand(IChannelDefinitionBuilder builder, Type messageType)
    {
        // TODO: implement
    }

    private void ConfigureChannelForOutgoingCommand(IChannelDefinitionBuilder builder, Type messageType)
    {
        // TODO: implement
    }
}
