using System;
using System.Threading.Tasks;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Testing;
using EasyDesk.Tools;
using NSubstitute;
using Shouldly;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Mediator;

public class NotificationHandlerBaseTests
{
    public class TestHandler : NotificationHandlerBase<int>
    {
        private readonly Func<int, Response<Nothing>> _handler;

        public TestHandler(Func<int, Response<Nothing>> handler)
        {
            _handler = handler;
        }

        public TestHandler(Func<int, Response<Nothing>> handler, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _handler = handler;
        }

        protected override Task<Response<Nothing>> Handle(int ev) => Task.FromResult(_handler(ev));
    }

    private const int Value = 10;
    private readonly Func<int, Response<Nothing>> _handlerCode;
    private readonly EventContext<int> _eventContext;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationHandlerBaseTests()
    {
        _handlerCode = Substitute.For<Func<int, Response<Nothing>>>();
        _handlerCode(Arg.Any<int>()).Returns(Ok);

        _eventContext = new(Value);

        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.Save().Returns(Ok);
    }

    private async Task Handle() =>
        await new TestHandler(_handlerCode).Handle(_eventContext, default);

    private async Task HandleWithUnitOfWork() =>
        await new TestHandler(_handlerCode, _unitOfWork).Handle(_eventContext, default);

    [Fact]
    public async Task Handle_ShouldCallTheHandlerCodeWithTheGivenEventData()
    {
        await Handle();

        _handlerCode.Received(1)(Value);
    }

    [Fact]
    public async Task Handle_ShouldNotCallTheHandlerCode_IfTheContextAlreadyContainsAnError()
    {
        _eventContext.SetError(TestError.Create());

        await Handle();

        _handlerCode.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public async Task Handle_ShouldSetAnErrorOnTheContext_IfTheHandlerCodeReturnsAnError()
    {
        var error = TestError.Create();
        _handlerCode(Arg.Any<int>()).Returns(error);

        await Handle();

        _eventContext.Error.ShouldContain(error);
    }

    [Fact]
    public async Task Handle_ShouldSaveTheUnitOfWork_IfItWasPassedThroughTheConstructorAndTheHandlerCodeWasSuccessful()
    {
        await HandleWithUnitOfWork();

        await _unitOfWork.Received(1).Save();
    }

    [Fact]
    public async Task Handle_ShouldSetAnErrorOnTheContext_IfSavingTheUnitOfWorkFails()
    {
        var error = TestError.Create();
        _unitOfWork.Save().Returns(error);

        await HandleWithUnitOfWork();

        _eventContext.Error.ShouldContain(error);
    }

    [Fact]
    public async Task Handle_ShouldNotSaveTheUnitOfWork_IfTheHandlerCodeFails()
    {
        var error = TestError.Create();
        _handlerCode(Arg.Any<int>()).Returns(error);

        await HandleWithUnitOfWork();

        await _unitOfWork.DidNotReceiveWithAnyArgs().Save();
    }
}
