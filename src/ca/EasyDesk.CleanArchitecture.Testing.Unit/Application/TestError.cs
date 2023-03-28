namespace EasyDesk.CleanArchitecture.Testing.Unit.Application;

public record TestError(string Code) : Error
{
    public static Error Create() => Create("ERROR_CODE");

    public static Error Create(string code) => new TestError(code);
}
