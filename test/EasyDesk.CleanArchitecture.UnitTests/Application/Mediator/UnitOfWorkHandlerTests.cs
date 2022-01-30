using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Testing;
using EasyDesk.Tools;
using NSubstitute;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Mediator;

public class UnitOfWorkHandlerTests
{
    public record TestCommand : CommandBase<Nothing>;

    public class TestHandler : UnitOfWorkHandler<TestCommand, Nothing>
    {
        private readonly Func<Response<Nothing>> _resultProvider;

        public TestHandler(Func<Response<Nothing>> resultProvider, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _resultProvider = resultProvider;
        }

        protected override Task<Response<Nothing>> HandleRequest(TestCommand request)
        {
            return Task.FromResult(_resultProvider());
        }
    }

    private readonly Func<Response<Nothing>> _resultProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TestHandler _sut;

    public UnitOfWorkHandlerTests()
    {
        _resultProvider = Substitute.For<Func<Response<Nothing>>>();
        _resultProvider().Returns(Ok);

        _unitOfWork = Substitute.For<IUnitOfWork>();

        _sut = new(_resultProvider, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldSaveTheUnitOfWorkAfterTheHandlerExecution()
    {
        await SendCommand();

        Received.InOrder(() =>
        {
            _resultProvider();
            _unitOfWork.Save();
        });
    }

    [Fact]
    public async Task Handle_ShouldReturnAnError_IfTheHandlerCodeReturnsAnError()
    {
        var response = HandlerFailedResponse();
        _resultProvider().Returns(response);

        var result = await SendCommand();

        result.ShouldBe(response);
    }

    [Fact]
    public async Task Handle_ShouldNotSaveTheUnitOfWork_IfTheHandlerCodeReturnsAnError()
    {
        _resultProvider().Returns(HandlerFailedResponse());

        await SendCommand();

        await _unitOfWork.DidNotReceive().Save();
    }

    private async Task<Response<Nothing>> SendCommand() => await _sut.Handle(new(), default);

    private static Response<Nothing> HandlerFailedResponse() => TestError.Create("HANDLER_FAILED");
}
