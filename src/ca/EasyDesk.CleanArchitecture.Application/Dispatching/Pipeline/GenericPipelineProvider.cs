using Autofac;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

internal class GenericPipelineProvider : IPipelineProvider
{
    private readonly ConcurrentDictionary<(Type, Type), IEnumerable<Type>> _pipelineCache = [];
    private readonly IImmutableList<Type> _stepTypes;

    public GenericPipelineProvider(IEnumerable<Type> stepTypes)
    {
        _stepTypes = stepTypes.ToImmutableList();
    }

    public IEnumerable<IPipelineStep<T, R>> GetSteps<T, R>(IComponentContext context)
    {
        var stepTypes = _pipelineCache.GetOrAdd((typeof(T), typeof(R)), _ => ComputePipelineStepTypes<T, R>());
        return stepTypes
            .Select(t => (IPipelineStep<T, R>)ActivatorUtilities.CreateInstance(context.Resolve<IServiceProvider>(), t));
    }

    private IEnumerable<Type> ComputePipelineStepTypes<T, R>() => _stepTypes.SelectMany(t => GetActualStepType<T, R>(t)).ToList();

    private Option<Type> GetActualStepType<T, R>(Type stepType)
    {
        return stepType
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineStep<,>))
            .SingleOption(() => new InvalidOperationException(
                $"Pipeline step of type {stepType.Name} implements more than one {typeof(IPipelineStep<,>).Name} interface."))
            .Filter(MatchesPipelineInterface<T, R>)
            .FlatMap(i => ConstructStepType<T, R>(stepType, i));
    }

    private bool MatchesPipelineInterface<T, R>(Type type)
    {
        var genericArgs = type.GetGenericArguments();
        return Matches<T>(genericArgs[0], (a, b) => a.IsAssignableTo(b))
            && Matches<R>(genericArgs[1], (a, b) => a == b);
    }

    private bool Matches<T>(Type argType, Func<Type, Type, bool> predicate)
    {
        return argType.IsGenericParameter || predicate(typeof(T), argType);
    }

    private Option<Type> ConstructStepType<T, R>(Type stepType, Type stepInterface)
    {
        if (!stepType.IsGenericTypeDefinition)
        {
            return Some(stepType);
        }

        var arguments = stepInterface
            .GetGenericArguments()
            .Zip(new[] { typeof(T), typeof(R), })
            .Where(a => a.First.IsGenericParameter)
            .Select(b => b.Second)
            .ToArray();
        try
        {
            return Some(stepType.MakeGenericType(arguments));
        }
        catch (ArgumentException)
        {
            return None;
        }
    }
}
