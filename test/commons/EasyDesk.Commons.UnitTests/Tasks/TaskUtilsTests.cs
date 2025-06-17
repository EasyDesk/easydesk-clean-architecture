using EasyDesk.Commons.Tasks;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Tasks;

public class TaskUtilsTests
{
    private const int Value = 0;

    private readonly Exception _exception = new IOException("FAILED");

    private Task<int> SuccessfulTask => YieldedTask(() => Value);

    private Task<int> FailedTask => YieldedTask(() => throw _exception);

    private async Task<int> YieldedTask(Func<int> result)
    {
        await Task.Yield();
        return result();
    }

    private async Task ShouldThrowTheExpectedException(Func<Task> actual)
    {
        var exception = await Should.ThrowAsync<Exception>(actual);
        exception.ShouldBeSameAs(_exception);
    }

    [Fact]
    public async Task Then_ShouldCallTheGivenAction_IfTheTaskIsSuccessful()
    {
        var action = Substitute.For<Action>();
        await SuccessfulTask.Then(action);
        action.Received(1)();
    }

    [Fact]
    public async Task ThenWithAsyncContinuation_ShouldCallTheGivenAsyncAction_IfTheTaskIsSuccessful()
    {
        var action = Substitute.For<AsyncAction>();
        await SuccessfulTask.Then(action);
        await action.Received(1)();
    }

    [Fact]
    public async Task Then_ShouldFail_IfTheTaskFails()
    {
        var action = Substitute.For<Action>();
        await ShouldThrowTheExpectedException(() => FailedTask.Then(action));
        action.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public async Task ThenWithAsyncContinuation_ShouldFail_IfTheTaskFails()
    {
        var action = Substitute.For<AsyncAction>();
        await ShouldThrowTheExpectedException(() => FailedTask.Then(action));
        await action.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public async Task ThenForTaskWithResult_ShouldCallTheGivenAction_IfTheTaskIsSuccessful()
    {
        var action = Substitute.For<Action<int>>();
        await SuccessfulTask.Then(action);
        action.Received(1)(Value);
    }

    [Fact]
    public async Task ThenForTaskWithResultAndAsyncContinuation_ShouldCallTheGivenAsyncAction_IfTheTaskIsSuccessful()
    {
        var action = Substitute.For<AsyncAction<int>>();
        await SuccessfulTask.Then(action);
        await action.Received(1)(Value);
    }

    [Fact]
    public async Task ThenForTaskWithResult_ShouldFail_IfTheTaskFails()
    {
        var action = Substitute.For<Action<int>>();
        await ShouldThrowTheExpectedException(() => FailedTask.Then(action));
        action.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public async Task ThenForTaskWithResultAndAsyncContinuation_ShouldFail_IfTheTaskFails()
    {
        var action = Substitute.For<AsyncAction<int>>();
        await ShouldThrowTheExpectedException(() => FailedTask.Then(action));
        await action.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public async Task Map_ShouldMapTheResultOfTheTask_IfTheTaskIsSuccessful()
    {
        var result = await SuccessfulTask.Map(x => x + 1);
        result.ShouldBe(Value + 1);
    }

    [Fact]
    public async Task Map_ShouldFail_IfTheTaskFails()
    {
        var mapper = Substitute.For<Func<int, int>>();
        await ShouldThrowTheExpectedException(() => FailedTask.Map(mapper));
        mapper.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public async Task MapForTaskWithoutResult_ShouldReturnTheGivenValue_IfTheTaskIsSuccessful()
    {
        var newValue = 5;
        var result = await SuccessfulTask.Map(() => newValue);
        result.ShouldBe(newValue);
    }

    [Fact]
    public async Task MapForTaskWithoutResult_ShouldFail_IfTheTaskFails()
    {
        var mapper = Substitute.For<Func<int>>();
        await ShouldThrowTheExpectedException(() => FailedTask.Map(mapper));
        mapper.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public async Task FlatMap_ShouldReturnTheGivenValueAsynchronously_IfBothTasksAreSuccessful()
    {
        var result = await SuccessfulTask.FlatMap(x => Task.FromResult(x + 1));
        result.ShouldBe(Value + 1);
    }

    [Fact]
    public async Task FlatMap_ShouldFail_IfTheReceiverTaskFails()
    {
        var mapper = Substitute.For<AsyncFunc<int, int>>();
        await ShouldThrowTheExpectedException(() => FailedTask.FlatMap(mapper));
        await mapper.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public async Task FlatMap_ShouldFail_IfTheMappedTaskFails()
    {
        await ShouldThrowTheExpectedException(() => SuccessfulTask.FlatMap(x => Task.FromException<int>(_exception)));
    }

    ////[Fact]
    ////public async Task FireAndForget_ShouldCallTheExceptionHandler_IfTheTaskFailsWithACompatibleException()
    ////{
    ////    var exceptionHandler = Substitute.For<Action<Exception>>();
    ////    FailedTask.FireAndForget(exceptionHandler);
    ////    await HandlingException(FailedTask);
    ////    exceptionHandler.Received(1)(_exception);
    ////}

    ////[Fact]
    ////public async Task FireAndForget_ShouldNotCallTheExceptionHandler_IfTheTaskFailsWithAnIncompatibleException()
    ////{
    ////    var exceptionHandler = Substitute.For<Action<InvalidOperationException>>();
    ////    FailedTask.FireAndForget(exceptionHandler);
    ////    await HandlingException(FailedTask);
    ////    exceptionHandler.DidNotReceiveWithAnyArgs()(default);
    ////}

    ////private async Task HandlingException(Task task)
    ////{
    ////    try
    ////    {
    ////        await task;
    ////    }
    ////    catch { }
    ////}
}
