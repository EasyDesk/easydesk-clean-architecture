using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

public class PipelineBuilder
{
    private readonly IList<Type> _steps = new List<Type>();
    private readonly ISet<(Type, Type)> _dependencies = new HashSet<(Type, Type)>();

    public StepDependenciesBuilder AddStep(Type stepType)
    {
        _steps.Add(stepType);
        return new StepDependenciesBuilder(this, stepType);
    }

    public PipelineBuilder AddDependency(Type before, Type after)
    {
        _dependencies.Add((before, after));
        return this;
    }

    public IEnumerable<Type> GetOrderedSteps()
    {
        if (_steps.IsEmpty())
        {
            return Enumerable.Empty<Type>();
        }

        var result = new List<Type>();
        var remaining = new List<Type>(_steps.Reverse());
        var temp = new HashSet<Type>();
        var successorsMap = _dependencies
            .Where(IsValidDependency)
            .GroupBy(d => d.Item1)
            .ToDictionary(x => x.Key, x => x.Select(d => d.Item2));

        while (remaining.Count > 0)
        {
            Visit(remaining[0]);
        }

        void Visit(Type step)
        {
            if (result.Contains(step))
            {
                return;
            }
            if (temp.Contains(step))
            {
                throw new Exception("Pipeline steps dependencies contain cycles");
            }

            temp.Add(step);

            var successors = successorsMap.GetOption(step).OrElseGet(Enumerable.Empty<Type>);
            foreach (var successor in successors)
            {
                Visit(successor);
            }

            temp.Remove(step);

            result.Add(step);
            remaining.Remove(step);
        }

        return result.AsEnumerable().Reverse();
    }

    private bool IsValidDependency((Type, Type) dependency) =>
        _steps.Contains(dependency.Item1) && _steps.Contains(dependency.Item2);
}

public class StepDependenciesBuilder
{
    private readonly PipelineBuilder _builder;
    private readonly Type _stepType;

    public StepDependenciesBuilder(PipelineBuilder builder, Type stepType)
    {
        _builder = builder;
        _stepType = stepType;
    }

    public StepDependenciesBuilder Before(Type other)
    {
        _builder.AddDependency(_stepType, other);
        return this;
    }

    public StepDependenciesBuilder After(Type other)
    {
        _builder.AddDependency(other, _stepType);
        return this;
    }
}
