using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Dispatching;

public class DispatcherTests
{
    public record IntRequest : IDispatchable<int>;

    public record StringRequest : IDispatchable<string>;

    public abstract class GenericStepBase<T, R> : IPipelineStep<T, R>
        where R : notnull
    {
        private readonly Action _before;
        private readonly Action _after;

        public GenericStepBase(Action before, Action after)
        {
            _before = before;
            _after = after;
        }

        public Task<Result<R>> Run(T request, NextPipelineStep<R> next)
        {
            _before();
            var result = next();
            _after();
            return result;
        }
    }

    public class GenericStepA<T, R> : GenericStepBase<T, R>
        where R : notnull
    {
        public GenericStepA(Action<string> notifier) : base(() => notifier("A1"), () => notifier("A2"))
        {
        }
    }

    public class GenericStepB<T, R> : GenericStepBase<T, R>
        where R : notnull
    {
        public GenericStepB(Action<string> notifier) : base(() => notifier("B1"), () => notifier("B2"))
        {
        }
    }

    private const int IntValue = 10;
    private const string StringValue = "hello";

    private readonly IPipeline _pipeline;

    private readonly IntRequest _intRequest = new();
    private readonly StringRequest _stringRequest = new();

    private readonly IHandler<IntRequest, int> _intHandler;
    private readonly IHandler<StringRequest, string> _stringHandler;

    public DispatcherTests()
    {
        _pipeline = Substitute.For<IPipeline>();
        _pipeline.GetSteps<StringRequest, string>(default!).ReturnsForAnyArgs(Enumerable.Empty<IPipelineStep<StringRequest, string>>());
        _pipeline.GetSteps<IntRequest, int>(default!).ReturnsForAnyArgs(Enumerable.Empty<IPipelineStep<IntRequest, int>>());

        _intHandler = Substitute.For<IHandler<IntRequest, int>>();
        _intHandler.Handle(_intRequest).Returns(Success(IntValue));

        _stringHandler = Substitute.For<IHandler<StringRequest, string>>();
        _stringHandler.Handle(_stringRequest).Returns(Success(StringValue));
    }

    private Dispatcher CreateDispatcher(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        configure?.Invoke(services);
        return new Dispatcher(services.BuildServiceProvider(), _pipeline);
    }

    [Fact]
    public async Task ShouldThrowAnExceptionIfNoHandlerExists()
    {
        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_stringHandler);
        });

        var exception = await Should.ThrowAsync<HandlerNotFoundException>(dispatcher.Dispatch(_intRequest));
        exception.RequestType.ShouldBe(typeof(IntRequest));
    }

    [Fact]
    public async Task ShouldNotCallThePipelineIfNoHandlerExists()
    {
        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_stringHandler);
        });

        await Should.ThrowAsync<HandlerNotFoundException>(dispatcher.Dispatch(_intRequest));

        _pipeline.DidNotReceiveWithAnyArgs().GetSteps<IntRequest, int>(default!);
    }

    [Fact]
    public async Task ShouldCallTheHandlerIfItExists()
    {
        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_intHandler);
        });

        await dispatcher.Dispatch(_intRequest);

        await _intHandler.Received(1).Handle(_intRequest);
    }

    [Fact]
    public async Task ShouldCallTheCorrectHandler_IfMultipleExist()
    {
        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_intHandler);
            services.AddSingleton(_stringHandler);
        });

        await dispatcher.Dispatch(_intRequest);

        await _intHandler.Received(1).Handle(_intRequest);
    }

    [Fact]
    public async Task ShouldReturnTheResultOfTheHandler()
    {
        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_intHandler);
        });

        var result = await dispatcher.Dispatch(_intRequest);

        result.ShouldBe(Success(IntValue));
    }

    [Fact]
    public async Task ShouldBeAbleToDispatchMultipleTimesWithTheSameRequestType()
    {
        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_intHandler);
        });

        await dispatcher.Dispatch(_intRequest);
        await dispatcher.Dispatch(_intRequest);
        await dispatcher.Dispatch(_intRequest);

        await _intHandler.Received(3).Handle(_intRequest);
    }

    [Fact]
    public async Task ShouldBeAbleToDispatchMultipleTimesWithDifferentRequestTypes()
    {
        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_intHandler);
            services.AddSingleton(_stringHandler);
        });

        await dispatcher.Dispatch(_intRequest);
        await dispatcher.Dispatch(_stringRequest);

        await _intHandler.Received(1).Handle(_intRequest);
        await _stringHandler.Received(1).Handle(_stringRequest);
    }

    [Fact]
    public async Task ShouldInvokePipelineStepsInOrder()
    {
        var step1 = SubstituteForPipelineStep<IntRequest, int>();
        var step2 = SubstituteForPipelineStep<IntRequest, int>();
        SetupPipeline(step1, step2);

        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_intHandler);
        });

        await dispatcher.Dispatch(_intRequest);

        Received.InOrder(() =>
        {
            step1.Run(_intRequest, Arg.Any<NextPipelineStep<int>>());
            step2.Run(_intRequest, Arg.Any<NextPipelineStep<int>>());
        });
    }

    [Fact]
    public async Task ShouldInvokeStepsWithTheCorrectNesting()
    {
        var notifier = Substitute.For<Action<string>>();
        var stepA = new GenericStepA<IntRequest, int>(notifier);
        var stepB = new GenericStepB<IntRequest, int>(notifier);
        SetupPipeline(stepA, stepB);

        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_intHandler);

            services.AddSingleton(notifier);
        });

        await dispatcher.Dispatch(_intRequest);

        Received.InOrder(() =>
        {
            notifier("A1");
            notifier("B1");
            _intHandler.Handle(_intRequest);
            notifier("B2");
            notifier("A2");
        });
    }

    private void SetupPipeline<T, R>(params IPipelineStep<T, R>[] steps)
        where R : notnull
    {
        _pipeline.GetSteps<T, R>(default!).ReturnsForAnyArgs(steps);
    }

    private IPipelineStep<T, R> SubstituteForPipelineStep<T, R>()
        where T : IDispatchable<R>
        where R : notnull
    {
        var step = Substitute.For<IPipelineStep<T, R>>();
        step.Run(default!, default!).ReturnsForAnyArgs(x => x.Arg<NextPipelineStep<R>>()());
        return step;
    }
}
