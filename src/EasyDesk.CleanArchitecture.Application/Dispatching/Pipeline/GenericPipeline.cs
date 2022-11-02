using EasyDesk.Tools.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

internal class GenericPipeline : IPipeline
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<Type> _stepTypes;

    public GenericPipeline(IServiceProvider serviceProvider, IEnumerable<Type> stepTypes)
    {
        _serviceProvider = serviceProvider;
        _stepTypes = stepTypes;
    }

    public IEnumerable<IPipelineStep<T, R>> GetSteps<T, R>() =>
        _stepTypes.SelectMany(t => ConvertTypeToStep<T, R>(t));

    private Option<IPipelineStep<T, R>> ConvertTypeToStep<T, R>(Type type)
    {
        return type
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineStep<,>))
            .SingleOption(() => new InvalidOperationException(
                $"Pipeline step of type {type.Name} implements more than one {typeof(IPipelineStep<,>).Name} interface."))
            .Filter(i => MatchesPipelineInterface<T, R>(i))
            .FlatMap(i => GetActualStepType<T, R>(type, i))
            .Map(t => (IPipelineStep<T, R>)ActivatorUtilities.CreateInstance(_serviceProvider, t));
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

    private Option<Type> GetActualStepType<T, R>(Type type, Type stepInterface)
    {
        if (!type.IsGenericTypeDefinition)
        {
            return Some(type);
        }

        var arguments = stepInterface
            .GetGenericArguments()
            .Zip(new[] { typeof(T), typeof(R) })
            .Where(a => a.First.IsGenericParameter)
            .Select(b => b.Second)
            .ToArray();
        try
        {
            return Some(type.MakeGenericType(arguments));
        }
        catch (ArgumentException)
        {
            return None;
        }
    }
}
