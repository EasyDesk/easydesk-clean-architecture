using EasyDesk.Tools.Results;

namespace EasyDesk.CleanArchitecture.Testing;

public record TestError(string Code) : Error
{
    public static Error Create() => Create("ERROR_CODE");

    public static Error Create(string code) => new TestError(code);
}
