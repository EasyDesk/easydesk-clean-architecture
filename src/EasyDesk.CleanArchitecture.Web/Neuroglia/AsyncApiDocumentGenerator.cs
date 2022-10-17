using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Reflection;
using Neuroglia.AsyncApi.Configuration;
using Neuroglia.AsyncApi.Models;
using Neuroglia.AsyncApi.Services.Generators;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Web.Neuroglia;
public class AsyncApiDocumentGenerator : IAsyncApiDocumentGenerator
{
    public Task<IEnumerable<AsyncApiDocument>> GenerateAsync(params Type[] markupTypes)
    {
        return this.GenerateAsync(markupTypes, null);
    }

    public Task<IEnumerable<AsyncApiDocument>> GenerateAsync(IEnumerable<Type> markupTypes, AsyncApiDocumentGenerationOptions options)
    {
        if (markupTypes == null)
        {
            throw new ArgumentNullException(nameof(markupTypes));
        }
        var types = ScanAssemblyForSubtypesOrImplementationsOf<IIncomingMessage, IOutgoingCommand, IOutgoingEvent>(markupTypes);

        // TODO: implement generation of async api documents
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
