using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Application.UserInfo;
using EasyDesk.CleanArchitecture.Testing;
using EasyDesk.Tools;
using NSubstitute;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Mediator
{
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
            _unitOfWork.Commit().Returns(Ok);

            _sut = new(_resultProvider, _unitOfWork);
        }

        [Fact]
        public async Task Handle_ShouldWrapTheHandlerCodeInsideAUnitOfWork()
        {
            await SendCommand();

            Received.InOrder(() =>
            {
                _unitOfWork.Begin();
                _resultProvider();
                _unitOfWork.Commit();
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
        public async Task Handle_ShouldNotCommitTheTransaction_IfTheHandlerCodeReturnsAnError()
        {
            _resultProvider().Returns(HandlerFailedResponse());

            await SendCommand();

            await _unitOfWork.DidNotReceive().Commit();
        }

        [Fact]
        public async Task Handle_ShouldReturnAnError_IfCommitReturnsAnError()
        {
            var response = CommitFailedResponse();
            _unitOfWork.Commit().Returns(response);

            var result = await SendCommand();

            result.ShouldBe(response);
        }

        private async Task<Response<Nothing>> SendCommand() => await _sut.Handle(new(new(), Substitute.For<IUserInfo>()), default);

        private static Response<Nothing> HandlerFailedResponse() => TestError.Create("HANDLER_FAILED");

        private static Response<Nothing> CommitFailedResponse() => TestError.Create("COMMIT_FAILED");
    }
}
