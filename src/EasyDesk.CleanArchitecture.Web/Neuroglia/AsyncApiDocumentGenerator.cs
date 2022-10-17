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

        List<AsyncApiDocument> documents = new(types.Types.Count());
        foreach (var type in types.Types)
        {
            documents.Add(await GenerateDocumentAsync(type, options));
        }
        return documents;
    }

    private Task<AsyncApiDocument> GenerateDocumentAsync(Type type, AsyncApiDocumentGenerationOptions options)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }
        IAsyncApiDocumentBuilder builder = _serviceProvider.GetRequiredService<IAsyncApiDocumentBuilder>();
        options?.DefaultConfiguration.Invoke(builder);
        builder
            .WithTitle(type.Name);
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
