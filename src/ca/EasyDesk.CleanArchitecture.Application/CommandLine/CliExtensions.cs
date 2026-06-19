using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.CommandLine;

public static class CliExtensions
{
    public static void RegisterCliCommand(this ContainerBuilder self, string name, Action<CliCommandBuilder> configure)
    {
        self.RegisterCliCommand(name, (builder, _) => configure(builder));
    }

    public static void RegisterCliCommand(this ContainerBuilder self, string name, Action<CliCommandBuilder, IComponentContext> configure)
    {
        self
            .Register(c =>
            {
                var componentContext = c.Resolve<IComponentContext>();
                var commandBuilder = new CliCommandBuilder(name, componentContext);
                configure(commandBuilder, componentContext);
                return commandBuilder.Build();
            })
            .InstancePerDependency();
    }

    public static void AddCliCommand(this IServiceCollection self, string name, Action<CliCommandBuilder> configure)
    {
        self.AddCliCommand(name, (builder, _) => configure(builder));
    }

    public static void AddCliCommand(this IServiceCollection self, string name, Action<CliCommandBuilder, IServiceProvider> configure)
    {
        self.AddTransient(sp =>
        {
            var componentContext = sp.GetRequiredService<IComponentContext>();
            var commandBuilder = new CliCommandBuilder(name, componentContext);
            configure(commandBuilder, sp);
            return commandBuilder.Build();
        });
    }
}
