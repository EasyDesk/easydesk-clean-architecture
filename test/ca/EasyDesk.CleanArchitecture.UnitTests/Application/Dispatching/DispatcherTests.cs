using Autofac;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using NSubstitute;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Dispatching;

public class DispatcherTests
{
    public record IntRequest : IDispatchable<int>;

    public record StringRequest : IDispatchable<string>;

    private const int IntValue = 10;
    private const string StringValue = "hello";

    private readonly IPipelineProvider _pipelineProvider;

    private readonly IntRequest _intRequest = new();
    private readonly StringRequest _stringRequest = new();

    private readonly IHandler<IntRequest, int> _intHandler;
    private readonly IHandler<StringRequest, string> _stringHandler;

    public DispatcherTests()
    {
        void ConfigurePipelineDelegation<T, R>()
        {
            _pipelineProvider.GetSteps<T, R>(default!).ReturnsForAnyArgs([]);
        }

        _pipelineProvider = Substitute.For<IPipelineProvider>();
        ConfigurePipelineDelegation<IntRequest, int>();
        ConfigurePipelineDelegation<StringRequest, string>();

        _intHandler = Substitute.For<IHandler<IntRequest, int>>();
        _intHandler.Handle(_intRequest).Returns(Success(IntValue));

        _stringHandler = Substitute.For<IHandler<StringRequest, string>>();
        _stringHandler.Handle(_stringRequest).Returns(Success(StringValue));
    }

    private IDispatcher CreateDispatcher(Action<ContainerBuilder>? configure = null)
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance(_pipelineProvider).SingleInstance();
        configure?.Invoke(builder);
        return new Dispatcher(builder.Build());
    }

    [Fact]
    public async Task Dispatch_ShouldThrowAnExceptionIfNoHandlerExists()
    {
        var dispatcher = CreateDispatcher(builder =>
        {
            builder.RegisterInstance(_stringHandler).SingleInstance();
        });

        var exception = await Should.ThrowAsync<HandlerNotFoundException>(dispatcher.Dispatch(_intRequest));
        exception.RequestType.ShouldBe(typeof(IntRequest));
    }

    [Fact]
    public async Task Dispatch_ShouldNotCallThePipelineIfNoHandlerExists()
    {
        var dispatcher = CreateDispatcher(builder =>
        {
            builder.RegisterInstance(_stringHandler).SingleInstance();
        });

        await Should.ThrowAsync<HandlerNotFoundException>(dispatcher.Dispatch(_intRequest));

        _pipelineProvider.DidNotReceiveWithAnyArgs().GetSteps<IntRequest, int>(default!);
    }

    [Fact]
    public async Task Dispatch_ShouldCallTheHandlerIfItExists()
    {
        var dispatcher = CreateDispatcher(builder =>
        {
            builder.RegisterInstance(_intHandler).SingleInstance();
        });

        await dispatcher.Dispatch(_intRequest);

        await _intHandler.Received(1).Handle(_intRequest);
    }

    [Fact]
    public async Task Dispatch_ShouldCallTheCorrectHandler_IfMultipleExist()
    {
        var dispatcher = CreateDispatcher(builder =>
        {
            builder.RegisterInstance(_intHandler).SingleInstance();
            builder.RegisterInstance(_stringHandler).SingleInstance();
        });

        await dispatcher.Dispatch(_intRequest);

        await _intHandler.Received(1).Handle(_intRequest);
    }

    [Fact]
    public async Task Dispatch_ShouldReturnTheResultOfTheHandler()
    {
        var dispatcher = CreateDispatcher(builder =>
        {
            builder.RegisterInstance(_intHandler).SingleInstance();
        });

        var result = await dispatcher.Dispatch(_intRequest);

        result.ShouldBe(Success(IntValue));
    }

    [Fact]
    public async Task Dispatch_ShouldBeAbleToDispatchMultipleTimesWithTheSameRequestType()
    {
        var dispatcher = CreateDispatcher(builder =>
        {
            builder.RegisterInstance(_intHandler).SingleInstance();
        });

        await dispatcher.Dispatch(_intRequest);
        await dispatcher.Dispatch(_intRequest);
        await dispatcher.Dispatch(_intRequest);

        await _intHandler.Received(3).Handle(_intRequest);
    }

    [Fact]
    public async Task Dispatch_ShouldBeAbleToDispatchMultipleTimesWithDifferentRequestTypes()
    {
        var dispatcher = CreateDispatcher(builder =>
        {
            builder.RegisterInstance(_intHandler).SingleInstance();
            builder.RegisterInstance(_stringHandler).SingleInstance();
        });

        await dispatcher.Dispatch(_intRequest);
        await dispatcher.Dispatch(_stringRequest);

        await _intHandler.Received(1).Handle(_intRequest);
        await _stringHandler.Received(1).Handle(_stringRequest);
    }
}
