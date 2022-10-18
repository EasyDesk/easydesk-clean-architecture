using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.AsyncApi.Configuration;
using Neuroglia.AsyncApi.Models;
using Neuroglia.AsyncApi.Services.FluentBuilders;
using Neuroglia.AsyncApi.Services.Generators;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Web.Neuroglia;
public class AsyncApiDocumentGenerator : IAsyncApiDocumentGenerator
{
    private readonly IServiceProvider _serviceProvider;

    public AsyncApiDocumentGenerator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<IEnumerable<AsyncApiDocument>> GenerateAsync(params Type[] markupTypes)
    {
        return GenerateAsync(markupTypes, null);
    }

    public virtual async Task<IEnumerable<AsyncApiDocument>> GenerateAsync(IEnumerable<Type> markupTypes, AsyncApiDocumentGenerationOptions options)
    {
        if (markupTypes == null)
        {
            throw new ArgumentNullException(nameof(markupTypes));
        }
        var types = ScanAssemblyForSubtypesOrImplementationsOf<IIncomingMessage, IOutgoingCommand, IOutgoingEvent>(markupTypes);
        return await GenerateDocumentsAsync(types, options);
    }

    private Task<IEnumerable<AsyncApiDocument>> GenerateDocumentsAsync(KnownMessageTypes types, AsyncApiDocumentGenerationOptions options)
    {
        if (types == null)
        {
            throw new ArgumentNullException(nameof(types));
        }
        IAsyncApiDocumentBuilder builder = _serviceProvider.GetRequiredService<IAsyncApiDocumentBuilder>();
        options?.DefaultConfiguration.Invoke(builder);
        builder
            .WithId("MICROSERVICE_NAME") // TODO
            .WithVersion("API_VERSION") // TODO
            .WithTitle("MICROSERVICE_VERBOSE_NAME"); // TODO

        // TODO: add license/contact info?

        // TODO: use rebus to fetch outgoing routing channels

        // TODO: use rebus to fetch information about the servers (message brokers)
        foreach (var messageType in types.Types.Select(x => x.IsSubtypeOrImplementationOf(typeof(IIncomingEvent))))
        {
            // link 1: https://github.com/neuroglia-io/AsyncApi/blob/5d46f9b3118237729c5645de901d4dc0480c1a41/src/Neuroglia.AsyncApi.Core/Services/Generators/AsyncApiDocumentGenerator.cs
            // link 2: https://github.com/neuroglia-io/AsyncApi#1-mark-your-services-with-adequate-attributes
            // link 3: https://www.asyncapi.com/docs/reference/specification/v2.0.0#a-namechannelsobjectachannels-object
        }
        return null;
    }

    private IImmutableSet<Type> ScanAssemblyForSubtypesOrImplementationsOf<T>(IEnumerable<Type> markers) => new AssemblyScanner()
        .FromAssembliesContaining(markers)
        .NonAbstract()
        .SubtypesOrImplementationsOf<T>()
        .FindTypes()
        .ToEquatableSet();

    private KnownMessageTypes ScanAssemblyForSubtypesOrImplementationsOf<T1, T2, T3>(IEnumerable<Type> markers) =>
        new(ScanAssemblyForSubtypesOrImplementationsOf<T1>(markers)
            .Union(ScanAssemblyForSubtypesOrImplementationsOf<T2>(markers))
            .Union(ScanAssemblyForSubtypesOrImplementationsOf<T3>(markers)));
}
