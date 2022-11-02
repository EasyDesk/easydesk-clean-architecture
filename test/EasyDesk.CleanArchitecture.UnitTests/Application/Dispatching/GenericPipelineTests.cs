using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using NSubstitute;
using Shouldly;
using Xunit;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Dispatching;

public class GenericPipelineTests
{
    private abstract record Request;

    private record SpecificRequest : Request;

    private abstract record BaseStep<T, R> : IPipelineStep<T, R>
    {
        public Task<Result<R>> Run(T request, NextPipelineStep<R> next) => throw new NotImplementedException();
    }

    private record OpenGenericStep<T, R> : BaseStep<T, R>;

    private record OpenGenericRequestStep<T> : BaseStep<T, Nothing>;

    private record OpenGenericResultStep<R> : BaseStep<Request, R>;

    private record OpenGenericResultStepSpecific<R> : BaseStep<SpecificRequest, R>;

    private record ClosedStep : BaseStep<Request, Nothing>;

    private record StepWithService<T, R>(TestService TestService) : BaseStep<T, R>;

    private record StepWithConstraints<T, R> : BaseStep<T, R>
        where T : Request
        where R : struct;

    private record TestService;

    private readonly IServiceProvider _serviceProvider;
    private readonly TestService _testService = new();

    public GenericPipelineTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        _serviceProvider.GetService(typeof(TestService)).Returns(_testService);
    }

    private GenericPipeline CreatePipeline(params Type[] stepTypes) =>
        new(_serviceProvider, stepTypes);

    [Fact]
    public void ShouldReturnPipelineStepsInOrder()
    {
        var pipeline = CreatePipeline(
            typeof(OpenGenericStep<,>),
            typeof(OpenGenericResultStep<>),
            typeof(OpenGenericRequestStep<>),
            typeof(ClosedStep));

        pipeline.GetSteps<Request, Nothing>().ShouldBe(Items<IPipelineStep<Request, Nothing>>(
            new OpenGenericStep<Request, Nothing>(),
            new OpenGenericResultStep<Nothing>(),
            new OpenGenericRequestStep<Request>(),
            new ClosedStep()));
    }

    [Fact]
    public void ShouldReturnStepsWithTheCorrectRequestType()
    {
        var pipeline = CreatePipeline(
            typeof(OpenGenericStep<,>),
            typeof(OpenGenericResultStep<>),
            typeof(OpenGenericRequestStep<>),
            typeof(ClosedStep));

        pipeline.GetSteps<int, Nothing>().ShouldBe(Items<IPipelineStep<int, Nothing>>(
            new OpenGenericStep<int, Nothing>(),
            new OpenGenericRequestStep<int>()));
    }

    [Fact]
    public void ShouldReturnStepsWithTheCorrectResponseType()
    {
        var pipeline = CreatePipeline(
            typeof(OpenGenericStep<,>),
            typeof(OpenGenericResultStep<>),
            typeof(OpenGenericRequestStep<>),
            typeof(ClosedStep));

        pipeline.GetSteps<Request, int>().ShouldBe(Items<IPipelineStep<Request, int>>(
            new OpenGenericStep<Request, int>(),
            new OpenGenericResultStep<int>()));
    }

    [Fact]
    public void ShouldReturnStepsWithContravariantRequestType()
    {
        var pipeline = CreatePipeline(
            typeof(OpenGenericResultStep<>),
            typeof(OpenGenericResultStepSpecific<>));

        pipeline.GetSteps<SpecificRequest, Nothing>().ShouldBe(Items<IPipelineStep<SpecificRequest, Nothing>>(
            new OpenGenericResultStep<Nothing>(),
            new OpenGenericResultStepSpecific<Nothing>()));
    }

    [Fact]
    public void ShouldReturnStepsWithServicesInjectedFromTheGivenServiceProvider()
    {
        var pipeline = CreatePipeline(typeof(StepWithService<,>));

        pipeline.GetSteps<Request, Nothing>().ShouldBe(Items<IPipelineStep<Request, Nothing>>(
            new StepWithService<Request, Nothing>(_testService)));
    }

    [Fact]
    public void ShouldReturnStepsFulfillingGenericConstraints()
    {
        var pipeline = CreatePipeline(typeof(StepWithConstraints<,>));

        pipeline.GetSteps<Request, Nothing>().ShouldBe(Items<IPipelineStep<Request, Nothing>>(
            new StepWithConstraints<Request, Nothing>()));
    }

    [Fact]
    public void ShouldNotReturnStepsConflictingWithRequestGenericConstraints()
    {
        var pipeline = CreatePipeline(typeof(StepWithConstraints<,>));

        pipeline.GetSteps<int, Nothing>().ShouldBe(Enumerable.Empty<IPipelineStep<int, Nothing>>());
    }

    [Fact]
    public void ShouldNotReturnStepsConflictingWithResultGenericConstraints()
    {
        var pipeline = CreatePipeline(typeof(StepWithConstraints<,>));

        pipeline.GetSteps<Request, Request>().ShouldBe(Enumerable.Empty<IPipelineStep<Request, Request>>());
    }
}
