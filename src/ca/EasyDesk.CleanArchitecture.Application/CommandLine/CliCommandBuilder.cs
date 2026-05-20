using Autofac;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.Commons.Tasks;
using System.CommandLine;

namespace EasyDesk.CleanArchitecture.Application.CommandLine;

public class CliCommandBuilder
{
    private readonly string _name;
    private readonly IComponentContext _componentContext;
    private string? _description;
    private Action<Command>? _configure;

    public CliCommandBuilder(string name, IComponentContext componentContext)
    {
        _name = name;
        _componentContext = componentContext;
    }

    public CliCommandBuilder AddDescription(string description)
    {
        _description = description;
        return this;
    }

    public CliCommandBuilder AddOption<T>(Option<T> option)
    {
        return ConfigureCommand(command => command.Options.Add(option));
    }

    public CliCommandBuilder AddArgument<T>(Argument<T> argument)
    {
        return ConfigureCommand(command => command.Arguments.Add(argument));
    }

    public CliCommandBuilder ConfigureCommand(Action<Command> configure)
    {
        _configure += configure;
        return this;
    }

    public CliCommandBuilder HandleWithUseCase<R>(Func<ParseResult, ICommandRequest<R>> useCaseFactory)
    {
        return ConfigureCommand(command => command.SetAction(async (parseResult, cancellationToken) =>
        {
            InitializeCliContext(parseResult, cancellationToken);

            await _componentContext
                .Resolve<IDispatcher>()
                .Dispatch(useCaseFactory(parseResult))
                .ThenThrowIfFailure();
        }));
    }

    public CliCommandBuilder HandleWith(AsyncAction<CliContext> action)
    {
        return ConfigureCommand(command => command.SetAction(async (parseResult, cancellationToken) =>
        {
            var context = InitializeCliContext(parseResult, cancellationToken);

            await action(context);
        }));
    }

    public CliCommandBuilder HandleWith(AsyncFunc<CliContext, int> action)
    {
        return ConfigureCommand(command => command.SetAction(async (parseResult, cancellationToken) =>
        {
            var context = InitializeCliContext(parseResult, cancellationToken);

            return await action(context);
        }));
    }

    private CliContext InitializeCliContext(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var context = new CliContext
        {
            ParseResult = parseResult,
            CancellationToken = cancellationToken,
        };
        _componentContext.Resolve<CliContextAccessor>().CliContext = Some(context);
        return context;
    }

    internal Command Build() => new Command(_name, _description).Also(_configure);
}
