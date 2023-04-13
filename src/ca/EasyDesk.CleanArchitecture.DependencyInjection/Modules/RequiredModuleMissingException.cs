namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public sealed class RequiredModuleMissingException : Exception
{
    public RequiredModuleMissingException(Type moduleType)
        : base($"Missing required module of type {moduleType.Name}")
    {
        ModuleType = moduleType;
    }

    public Type ModuleType { get; }
}
