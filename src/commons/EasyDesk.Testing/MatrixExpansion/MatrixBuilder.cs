using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace EasyDesk.Testing.MatrixExpansion;

public class MatrixBuilder<T1> : MatrixBuilderBase<T1, MatrixBuilder<T1>>
{
    public MatrixBuilder(IEnumerable<T1> axis) : base(ImmutableStack.Create<Expansion>(_ => axis.Cast<object>()))
    {
    }

    public MatrixBuilder<T1, T2> Axis<T2>(IEnumerable<T2> axis) => GeneratedAxis(_ => axis);

    public MatrixBuilder<T1, T2> Axis<T2>(params T2[] axis) => Axis(axis.AsEnumerable());

    public MatrixBuilder<T1, T2> GeneratedAxis<T2>(Func<T1, IEnumerable<T2>> axis) => new(AddExpansion(axis));

    public MatrixBuilder<T1, T2> GeneratedValue<T2>(Func<T1, T2> value) => GeneratedAxis(x => new T2[] { value(x) });

    public MatrixBuilder<T1, T2> Fixed<T2>(T2 value) => GeneratedValue(_ => value);

    protected override T1 AsTuple(Func<object> next) => (T1)next();
}

public class MatrixBuilder<T1, T2> : MatrixBuilderBase<(T1, T2), MatrixBuilder<T1, T2>>
{
    public MatrixBuilder(IImmutableStack<Expansion> expansions) : base(expansions)
    {
    }

    public MatrixBuilder<T1, T2, T3> Axis<T3>(IEnumerable<T3> axis) => GeneratedAxis(_ => axis);

    public MatrixBuilder<T1, T2, T3> Axis<T3>(params T3[] axis) => Axis(axis.AsEnumerable());

    public MatrixBuilder<T1, T2, T3> GeneratedAxis<T3>(Func<(T1, T2), IEnumerable<T3>> axis) => new(AddExpansion(axis));

    public MatrixBuilder<T1, T2, T3> GeneratedValue<T3>(Func<(T1, T2), T3> value) => GeneratedAxis(x => new T3[] { value(x) });

    public MatrixBuilder<T1, T2, T3> Fixed<T3>(T3 value) => GeneratedValue(_ => value);

    protected override (T1, T2) AsTuple(Func<object> next) => ((T1)next(), (T2)next());
}

public class MatrixBuilder<T1, T2, T3> : MatrixBuilderBase<(T1, T2, T3), MatrixBuilder<T1, T2, T3>>
{
    public MatrixBuilder(IImmutableStack<Expansion> expansions) : base(expansions)
    {
    }

    public MatrixBuilder<T1, T2, T3, T4> Axis<T4>(IEnumerable<T4> axis) => GeneratedAxis(_ => axis);

    public MatrixBuilder<T1, T2, T3, T4> Axis<T4>(params T4[] axis) => Axis(axis.AsEnumerable());

    public MatrixBuilder<T1, T2, T3, T4> GeneratedAxis<T4>(Func<(T1, T2, T3), IEnumerable<T4>> axis) => new(AddExpansion(axis));

    public MatrixBuilder<T1, T2, T3, T4> GeneratedValue<T4>(Func<(T1, T2, T3), T4> value) => GeneratedAxis(x => new T4[] { value(x) });

    public MatrixBuilder<T1, T2, T3, T4> Fixed<T4>(T4 value) => GeneratedValue(_ => value);

    protected override (T1, T2, T3) AsTuple(Func<object> next) => ((T1)next(), (T2)next(), (T3)next());
}

public class MatrixBuilder<T1, T2, T3, T4> : MatrixBuilderBase<(T1, T2, T3, T4), MatrixBuilder<T1, T2, T3, T4>>
{
    public MatrixBuilder(IImmutableStack<Expansion> expansions) : base(expansions)
    {
    }

    protected override (T1, T2, T3, T4) AsTuple(Func<object> next) => ((T1)next(), (T2)next(), (T3)next(), (T4)next());
}
