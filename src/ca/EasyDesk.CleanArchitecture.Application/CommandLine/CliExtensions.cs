using Autofac;

namespace EasyDesk.CleanArchitecture.Application.CommandLine;

public static class CliExtensions
{
    extension(ContainerBuilder self)
    {
        public void RegisterCliCommand(string name, Action<CliCommandBuilder> configure)
        {
            self
                .Register(c => new CliCommandBuilder(name, c.Resolve<IComponentContext>()).Also(configure).Build())
                .InstancePerDependency();
        }

        public void RegisterCliCommand(string name, Action<CliCommandBuilder, IComponentContext> configure)
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
    }
}
