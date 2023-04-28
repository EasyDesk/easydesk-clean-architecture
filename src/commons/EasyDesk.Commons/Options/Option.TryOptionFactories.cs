using System.Diagnostics.CodeAnalysis;

namespace EasyDesk.Commons;

public static partial class StaticImports
{
    public delegate bool TryOperator<T>([MaybeNullWhen(false)] out T output);

    public delegate bool TryOperator<T1, T>(T1 arg1, [MaybeNullWhen(false)] out T output);

    public delegate bool TryOperator<T1, T2, T>(T1 arg1, T2 arg2, [MaybeNullWhen(false)] out T output);

    public delegate bool TryOperator<T1, T2, T3, T>(T1 arg1, T2 arg2, T3 arg3, [MaybeNullWhen(false)] out T output);

    public delegate bool TryOperator<T1, T2, T3, T4, T>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, [MaybeNullWhen(false)] out T output);

    public static Option<T> TryOption<T>(TryOperator<T> tryOperator) =>
        tryOperator(out var output) ? Some(output) : None;

    public static Option<T> TryOption<T1, T>(TryOperator<T1, T> tryOperator, T1 arg1) =>
        tryOperator(arg1, out var output) ? Some(output) : None;

    public static Option<T> TryOption<T1, T2, T>(TryOperator<T1, T2, T> tryOperator, T1 arg1, T2 arg2) =>
        tryOperator(arg1, arg2, out var output) ? Some(output) : None;

    public static Option<T> TryOption<T1, T2, T3, T>(TryOperator<T1, T2, T3, T> tryOperator, T1 arg1, T2 arg2, T3 arg3) =>
        tryOperator(arg1, arg2, arg3, out var output) ? Some(output) : None;

    public static Option<T> TryOption<T1, T2, T3, T4, T>(TryOperator<T1, T2, T3, T4, T> tryOperator, T1 arg1, T2 arg2, T3 arg3, T4 arg4) =>
        tryOperator(arg1, arg2, arg3, arg4, out var output) ? Some(output) : None;
}
