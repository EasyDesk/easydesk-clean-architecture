using EasyDesk.Tools.Collections;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;

public class MediatrPipelineBuilder
{
    private readonly IList<Type> _behaviors = new List<Type>();
    private readonly ISet<(Type, Type)> _dependencies = new HashSet<(Type, Type)>();

    public BehaviorDependenciesBuilder AddBehavior(Type behaviorType)
    {
        _behaviors.Add(behaviorType);
        return new BehaviorDependenciesBuilder(this, behaviorType);
    }

    public MediatrPipelineBuilder AddDependency(Type before, Type after)
    {
        _dependencies.Add((before, after));
        return this;
    }

    public IEnumerable<Type> GetOrderedBehaviors()
    {
        if (_behaviors.IsEmpty())
        {
            return Enumerable.Empty<Type>();
        }

        var result = new List<Type>();
        var remaining = new List<Type>(_behaviors.Reverse());
        var temp = new HashSet<Type>();
        var successorsMap = _dependencies
            .Where(IsValidDependency)
            .GroupBy(d => d.Item1)
            .ToDictionary(x => x.Key, x => x.Select(d => d.Item2));

        while (remaining.Count > 0)
        {
            Visit(remaining[0]);
        }

        void Visit(Type behavior)
        {
            if (result.Contains(behavior))
            {
                return;
            }
            if (temp.Contains(behavior))
            {
                throw new Exception("Pipeline behaviors dependencies contain cycles");
            }

            temp.Add(behavior);

            var successors = successorsMap.GetOption(behavior).OrElseGet(Enumerable.Empty<Type>);
            foreach (var successor in successors)
            {
                Visit(successor);
            }

            temp.Remove(behavior);

            result.Add(behavior);
            remaining.Remove(behavior);
        }

        return result.AsEnumerable().Reverse();
    }

    private bool IsValidDependency((Type, Type) dependency) =>
        _behaviors.Contains(dependency.Item1) && _behaviors.Contains(dependency.Item2);
}

public class BehaviorDependenciesBuilder
{
    private readonly MediatrPipelineBuilder _builder;
    private readonly Type _behaviorType;

    public BehaviorDependenciesBuilder(MediatrPipelineBuilder builder, Type behaviorType)
    {
        _builder = builder;
        _behaviorType = behaviorType;
    }

    public BehaviorDependenciesBuilder Before(Type other)
    {
        _builder.AddDependency(_behaviorType, other);
        return this;
    }

    public BehaviorDependenciesBuilder After(Type other)
    {
        _builder.AddDependency(other, _behaviorType);
        return this;
    }
}
