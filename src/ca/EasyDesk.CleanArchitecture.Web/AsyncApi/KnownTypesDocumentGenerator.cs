using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using Saunter.AsyncApiSchema.v2.Traits;
using Saunter.Generation;
using Saunter.Generation.Filters;
using Saunter.Generation.SchemaGeneration;
using System.Collections.Immutable;
using System.Net.Mime;
using System.Reflection;
using RebusHeaders = Rebus.Messages.Headers;

namespace EasyDesk.CleanArchitecture.Web.AsyncApi;

internal partial class KnownTypesDocumentGenerator : IDocumentGenerator
{
    public const string Version = "1.0.0";
    private readonly string _address;

    public KnownTypesDocumentGenerator(RebusEndpoint endpoint)
    {
        _address = endpoint.InputQueueAddress;
    }

    public string ServerName => $"Rebus @ {_address}";

    public AsyncApiDocument GenerateDocument(TypeInfo[] asyncApiTypes, AsyncApiOptions options, AsyncApiDocument prototype, IServiceProvider serviceProvider)
    {
        options.SchemaOptions.DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull;
        options.SchemaOptions.TypeMappers.Add(new OptionTypeMapper());

        var asyncApiSchema = prototype.Clone();

        var schemaResolver = new AsyncApiSchemaResolver(asyncApiSchema, options.SchemaOptions);

        var generator = new JsonSchemaGenerator(options.SchemaOptions);
        ConfigureDocument(asyncApiSchema, asyncApiTypes, options.SchemaOptions);

        var filterContext = new DocumentFilterContext(asyncApiTypes, schemaResolver, generator);
        foreach (var filterType in options.DocumentFilters)
        {
            var filter = (IDocumentFilter)serviceProvider.GetRequiredService(filterType);
            filter?.Apply(asyncApiSchema, filterContext);
        }

        return asyncApiSchema;
    }

    private void ConfigureDocument(AsyncApiDocument asyncApiSchema, IEnumerable<TypeInfo> asyncApiTypes, AsyncApiSchemaOptions schemaOptions)
    {
        asyncApiSchema.DefaultContentType = MediaTypeNames.Application.Json;

        asyncApiSchema.Servers[ServerName] = ConfigureServer();

        foreach (var messageType in asyncApiTypes.OrderBy(t => t.Name))
        {
            var channel = new ChannelItem();
            asyncApiSchema.Channels[messageType.Name] = channel;
            channel.Servers.Add(ServerName);
            if (messageType.IsSubtypeOrImplementationOf(typeof(IIncomingCommand)))
            {
                channel.Subscribe = ConfigureOperation(messageType, "Command", schemaOptions);
            }
            if (messageType.IsSubtypeOrImplementationOf(typeof(IOutgoingEvent)))
            {
                channel.Publish = ConfigureOperation(messageType, "Event", schemaOptions);
            }
        }
    }

    private Operation ConfigureOperation(
        Type messageType,
        string messageClassifier,
        AsyncApiSchemaOptions schemaOptions) => new()
        {
            OperationId = messageType.Name,
            Traits = [new OperationTrait { Summary = messageClassifier }],
            Message = ConfigureMessage(messageType, schemaOptions),
        };

    private Message ConfigureMessage(Type messageType, AsyncApiSchemaOptions schemaOptions)
    {
        var headerNames = typeof(RebusHeaders)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .SelectMany(fi => fi.GetRawConstantValue() is null ? None : (fi.GetRawConstantValue() as string).AsSome());
        var rebusHeadersDictionary = headerNames.Select(name => KeyValuePair.Create(name, string.Empty)).ToImmutableDictionary();
        var frameworkHeadersDictionary = rebusHeadersDictionary
            .Add(MultitenantMessagingUtils.TenantIdHeader, string.Empty);
        var headersSchema = JsonSchema.FromSampleJson(JsonConvert.SerializeObject(frameworkHeadersDictionary));
        headersSchema.Properties.ForEach(p => p.Value.IsRequired = false);
        foreach (var property in new[] { RebusHeaders.ContentType, RebusHeaders.MessageId, RebusHeaders.SentTime, })
        {
            headersSchema.Properties[property].IsRequired = true;
        }
        return new()
        {
            Name = messageType.Name,
            Title = PascalCaseSplitter.Split(messageType.Name),
            Payload = JsonSchema.FromType(messageType, schemaOptions),
            Headers = headersSchema,
        };
    }

    private Server ConfigureServer() => new(url: "https://github.com/rebus-org/Rebus", protocol: "https")
    {
        Description = $"Use \"{_address}\" as the name of the routing destination " +
            "for commands directed to this service, within Rebus router.",
    };
}
