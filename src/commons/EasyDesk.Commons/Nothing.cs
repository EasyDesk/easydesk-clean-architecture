namespace EasyDesk.Commons;

public readonly record struct Nothing
{
    public static Nothing Value { get; }

    public static Task<Nothing> ValueAsync { get; } = Task.FromResult(Value);

    public override string ToString() => nameof(Nothing);
}
