using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;
using NSubstitute;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Dispatching;

public class PipelineTests
{
    public record StringRequest : IDispatchable<string>;

    public abstract class GenericStepBase<T, R> : IPipelineStep<T, R>
    {
        private readonly Action _before;
        private readonly Action _after;

        public GenericStepBase(Action before, Action after)
        {
            _before = before;
            _after = after;
        }

        public bool IsForEachHandler => true;

        public Task<Result<R>> Run(T request, NextPipelineStep<R> next)
        {
            _before();
            var result = next();
            _after();
            return result;
        }
    }

    public class GenericStepA<T, R> : GenericStepBase<T, R>
    {
        public GenericStepA(Action<string> notifier) : base(() => notifier("A1"), () => notifier("A2"))
        {
        }
    }

    public class GenericStepB<T, R> : GenericStepBase<T, R>
    {
        public GenericStepB(Action<string> notifier) : base(() => notifier("B1"), () => notifier("B2"))
        {
        }
    }

    private const string ActionResult = "Hello";

    private readonly StringRequest _stringRequest = new();
    private readonly IPipelineProvider _pipelineProvider = Substitute.For<IPipelineProvider>();
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
    private readonly AsyncFunc<StringRequest, Result<string>> _action = Substitute.For<AsyncFunc<StringRequest, Result<string>>>();

    public PipelineTests()
    {
        _action(_stringRequest).Returns(ActionResult);
    }

    [Fact]
    public async Task ShouldInvokePipelineStepsInOrder()
    {
        var step1 = SubstituteForPipelineStep<StringRequest, string>();
        var step2 = SubstituteForPipelineStep<StringRequest, string>();
        SetupPipeline(step1, step2);

        await _pipelineProvider.GetSteps<StringRequest, string>(_serviceProvider).Run(_stringRequest, _action);

        Received.InOrder(() =>
        {
            step1.Run(_stringRequest, Arg.Any<NextPipelineStep<string>>());
            step2.Run(_stringRequest, Arg.Any<NextPipelineStep<string>>());
        });
    }

    [Fact]
    public async Task ShouldInvokeStepsWithTheCorrectNesting()
    {
        var notifier = Substitute.For<Action<string>>();
        var stepA = new GenericStepA<StringRequest, string>(notifier);
        var stepB = new GenericStepB<StringRequest, string>(notifier);
        SetupPipeline(stepA, stepB);

        await _pipelineProvider.GetSteps<StringRequest, string>(_serviceProvider).Run(_stringRequest, _action);

        Received.InOrder(() =>
        {
            notifier("A1");
            notifier("B1");
            _action(_stringRequest);
            notifier("B2");
            notifier("A2");
        });
    }

    [Fact]
    public async Task ShouldReturnTheActionResult()
    {
        var step1 = SubstituteForPipelineStep<StringRequest, string>();
        var step2 = SubstituteForPipelineStep<StringRequest, string>();
        SetupPipeline(step1, step2);

        var result = await _pipelineProvider.GetSteps<StringRequest, string>(_serviceProvider).Run(_stringRequest, _action);

        result.ShouldBe(ActionResult);
    }

    private void SetupPipeline<T, R>(params IPipelineStep<T, R>[] steps)
    {
        _pipelineProvider.GetSteps<T, R>(_serviceProvider).Returns(steps);
    }

    private IPipelineStep<T, R> SubstituteForPipelineStep<T, R>()
        where T : IDispatchable<R>
    {
        var step = Substitute.For<IPipelineStep<T, R>>();
        step.Run(default!, default!).ReturnsForAnyArgs(x => x.Arg<NextPipelineStep<R>>()());
        return step;
    }
}
