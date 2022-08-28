using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Handlers;
using EasyDesk.CleanArchitecture.Application.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Cqrs;

public class CqrsRequestDispatcherTests
{
    public record IntRequest : ICqrsRequest<int>;

    public record StringRequest : ICqrsRequest<string>;

    public abstract class GenericStepBase<TRequest, TResult> : IPipelineStep<TRequest, TResult>
        where TRequest : ICqrsRequest<TResult>
    {
        private readonly Action _before;
        private readonly Action _after;

        public GenericStepBase(Action before, Action after)
        {
            _before = before;
            _after = after;
        }

        public Task<Result<TResult>> Run(TRequest request, NextPipelineStep<TResult> next)
        {
            _before();
            var result = next();
            _after();
            return result;
        }
    }

    public class GenericStepA<TRequest, TResult> : GenericStepBase<TRequest, TResult>
        where TRequest : ICqrsRequest<TResult>
    {
        public GenericStepA(Action<string> notifier) : base(() => notifier("A1"), () => notifier("A2"))
        {
        }
    }

    public class GenericStepB<TRequest, TResult> : GenericStepBase<TRequest, TResult>
        where TRequest : ICqrsRequest<TResult>
    {
        public GenericStepB(Action<string> notifier) : base(() => notifier("B1"), () => notifier("B2"))
        {
        }
    }

    private const int IntValue = 10;
    private const string StringValue = "hello";

    private readonly IntRequest _intRequest = new();
    private readonly StringRequest _stringRequest = new();

    private readonly ICqrsRequestHandler<IntRequest, int> _intHandler;
    private readonly ICqrsRequestHandler<StringRequest, string> _stringHandler;

    public CqrsRequestDispatcherTests()
    {
        _intHandler = Substitute.For<ICqrsRequestHandler<IntRequest, int>>();
        _intHandler.Handle(_intRequest).Returns(Success(IntValue));

        _stringHandler = Substitute.For<ICqrsRequestHandler<StringRequest, string>>();
        _stringHandler.Handle(_stringRequest).Returns(Success(StringValue));
    }

    private CqrsRequestDispatcher CreateDispatcher(Action<IServiceCollection> configure = null)
    {
        var services = new ServiceCollection();
        configure(services);
        return new CqrsRequestDispatcher(services.BuildServiceProvider());
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
    public async Task ShouldInvokePipelineSteps()
    {
        var step = SubstituteForPipelineStep<IntRequest, int>();

        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_intHandler);

            services.AddSingleton(step);
        });

        await dispatcher.Dispatch(_intRequest);

        await step.Received(1).Run(_intRequest, Arg.Any<NextPipelineStep<int>>());
    }

    [Fact]
    public async Task ShouldInvokePipelineStepsInOrder()
    {
        var step1 = SubstituteForPipelineStep<IntRequest, int>();
        var step2 = SubstituteForPipelineStep<IntRequest, int>();

        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_intHandler);

            services.AddSingleton(step1);
            services.AddSingleton(step2);
        });

        await dispatcher.Dispatch(_intRequest);

        Received.InOrder(() =>
        {
            step1.Run(_intRequest, Arg.Any<NextPipelineStep<int>>());
            step2.Run(_intRequest, Arg.Any<NextPipelineStep<int>>());
        });
    }

    [Fact]
    public async Task ShouldInvokePipelineStepsOfTheCorrectType()
    {
        var stepInt = SubstituteForPipelineStep<IntRequest, int>();
        var stepString = SubstituteForPipelineStep<StringRequest, string>();

        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_intHandler);

            services.AddSingleton(stepInt);
            services.AddSingleton(stepString);
        });

        await dispatcher.Dispatch(_intRequest);

        await stepInt.Received(1).Run(_intRequest, Arg.Any<NextPipelineStep<int>>());
        await stepString.DidNotReceiveWithAnyArgs().Run(default, default);
    }

    [Fact]
    public async Task ShouldInvokeOpenGenericSteps()
    {
        var notifier = Substitute.For<Action<string>>();

        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_intHandler);

            services.AddSingleton(notifier);

            services.AddTransient(typeof(IPipelineStep<,>), typeof(GenericStepA<,>));
        });

        await dispatcher.Dispatch(_intRequest);

        Received.InOrder(() =>
        {
            notifier("A1");
            notifier("A2");
        });
    }

    [Fact]
    public async Task ShouldInvokeStepsWithTheCorrectNesting()
    {
        var notifier = Substitute.For<Action<string>>();

        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton(_intHandler);

            services.AddSingleton(notifier);

            services.AddTransient(typeof(IPipelineStep<,>), typeof(GenericStepA<,>));
            services.AddTransient(typeof(IPipelineStep<,>), typeof(GenericStepB<,>));
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

    private IPipelineStep<TRequest, TResult> SubstituteForPipelineStep<TRequest, TResult>()
        where TRequest : ICqrsRequest<TResult>
    {
        var step = Substitute.For<IPipelineStep<TRequest, TResult>>();
        step.Run(default, default).ReturnsForAnyArgs(x => x.Arg<NextPipelineStep<TResult>>()());
        return step;
    }
}
