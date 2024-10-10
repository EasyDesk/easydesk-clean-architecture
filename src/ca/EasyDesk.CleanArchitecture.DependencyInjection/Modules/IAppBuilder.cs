using System.Reflection;

namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public interface IAppBuilder
{
    IAppBuilder WithName(string name);

    IAppBuilder WithAssemblies(IEnumerable<Assembly> assemblies);

    IAppBuilder AddModule<T>(T module) where T : AppModule;

    IAppBuilder RemoveModule<T>() where T : AppModule;

    IAppBuilder ConfigureModule<T>(Action<T> configure) where T : AppModule;

    Task Run();
}

public static class AppBuilderExtensions
{
    public static IAppBuilder AddModule<T>(this IAppBuilder builder) where T : AppModule, new() =>
        builder.AddModule(new T());

    public static IAppBuilder WithAssemblies(this IAppBuilder builder, params Assembly[] assemblies) =>
        builder.WithAssemblies(assemblies.AsEnumerable());
}
