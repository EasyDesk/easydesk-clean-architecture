﻿using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

public sealed class PipelineBuilder
{
    private readonly ISet<Type> _registeredSteps = new HashSet<Type>();
    private readonly IList<Type> _beforeAll = [];
    private readonly IList<Type> _middleSteps = [];
    private readonly IList<Type> _afterAll = [];
    private readonly ISet<(Type, Type)> _dependencies = new HashSet<(Type, Type)>();

    public StepDependenciesBuilder AddStep(Type stepType) =>
        AddStepToList(stepType, _middleSteps);

    public StepDependenciesBuilder AddStepBeforeAll(Type stepType) =>
        AddStepToList(stepType, _beforeAll);

    public StepDependenciesBuilder AddStepAfterAll(Type stepType) =>
        AddStepToList(stepType, _afterAll);

    private StepDependenciesBuilder AddStepToList(Type stepType, IList<Type> list)
    {
        if (_registeredSteps.Contains(stepType))
        {
            throw new ArgumentException($"A pipeline step of type {stepType} has already been registered.", nameof(stepType));
        }
        _registeredSteps.Add(stepType);
        list.Add(stepType);
        return new(this, stepType);
    }

    public PipelineBuilder AddDependency(Type before, Type after)
    {
        _dependencies.Add((before, after));
        return this;
    }

    public IEnumerable<Type> GetOrderedSteps()
    {
        return OrderStepList(_beforeAll)
            .Concat(OrderStepList(_middleSteps))
            .Concat(OrderStepList(_afterAll));
    }

    private IEnumerable<Type> OrderStepList(IList<Type> steps)
    {
        if (steps.IsEmpty())
        {
            return [];
        }

        var result = new List<Type>();
        var remaining = new List<Type>(steps.Reverse());
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
                throw new InvalidOperationException("Pipeline steps dependencies contain cycles");
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
        _registeredSteps.Contains(dependency.Item1) && _registeredSteps.Contains(dependency.Item2);
}

public sealed class StepDependenciesBuilder
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
