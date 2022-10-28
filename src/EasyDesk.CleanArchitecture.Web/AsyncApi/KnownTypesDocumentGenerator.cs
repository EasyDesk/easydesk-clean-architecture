using System.Net.Mime;
using System.Reflection;
using System.Text.RegularExpressions;
using EasyDesk.CleanArchitecture.Application.Messaging.Messages;
using EasyDesk.Tools.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;
using NJsonSchema.Generation;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using Saunter.AsyncApiSchema.v2.Traits;
using Saunter.Generation;
using Saunter.Generation.Filters;
using Saunter.Generation.SchemaGeneration;

namespace EasyDesk.CleanArchitecture.Web.AsyncApi;

internal class KnownTypesDocumentGenerator : IDocumentGenerator
{
    public const string Version = "1.0.0";
    private readonly string _microserviceName;
    private readonly string _address;

    public KnownTypesDocumentGenerator(
        string microserviceName,
        string address)
    {
        _microserviceName = microserviceName;
        _address = address;
    }

    public string ServerName => $"Rebus @ {_address}";

    public AsyncApiDocument GenerateDocument(TypeInfo[] asyncApiTypes, AsyncApiOptions options, AsyncApiDocument prototype, IServiceProvider serviceProvider)
    {
        options.SchemaOptions.DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull;
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

    private void ConfigureDocument(AsyncApiDocument asyncApiSchema, TypeInfo[] asyncApiTypes, AsyncApiSchemaOptions schemaOptions)
    {
        asyncApiSchema.DefaultContentType = MediaTypeNames.Application.Json;
        asyncApiSchema.Info = new Info(_microserviceName, Version);

        asyncApiSchema.Servers[ServerName] = ConfigureServer();

        foreach (var messageType in asyncApiTypes)
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
            Traits = new List<IOperationTrait>() { new OperationTrait { Summary = messageClassifier } },
            Message = ConfigureMessage(messageType, schemaOptions)
        };

    private static string SplitPascalCase(string pascalString) => Regex.Replace(pascalString, "(?!^)([A-Z])", " $1");

    private Message ConfigureMessage(Type messageType, AsyncApiSchemaOptions schemaOptions) => new()
    {
        Name = messageType.Name,
        Title = SplitPascalCase(messageType.Name),
        Payload = JsonSchema.FromType(messageType, schemaOptions)
    };

    private Server ConfigureServer() => new(url: "https://github.com/rebus-org/Rebus", protocol: "https")
    {
        Description = $"Use \"{_address}\" as the name of the routing destination " +
            "for commands directed to this service, within Rebus router."
    };
}
