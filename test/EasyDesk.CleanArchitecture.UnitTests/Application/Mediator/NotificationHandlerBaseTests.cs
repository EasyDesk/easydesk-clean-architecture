using System;
using System.Threading.Tasks;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Testing;
using EasyDesk.Tools;
using NSubstitute;
using Shouldly;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Mediator
{
    public class NotificationHandlerBaseTests
    {
        public class TestHandler : NotificationHandlerBase<int>
        {
            private readonly Func<int, Response<Nothing>> _handler;

            public TestHandler(Func<int, Response<Nothing>> handler)
            {
                _handler = handler;
            }

            protected override Task<Response<Nothing>> Handle(int ev) => Task.FromResult(_handler(ev));
        }

        private const int Value = 10;
        private readonly Func<int, Response<Nothing>> _handlerCode;
        private readonly TestHandler _sut;

        public NotificationHandlerBaseTests()
        {
            _handlerCode = Substitute.For<Func<int, Response<Nothing>>>();
            _handlerCode(Arg.Any<int>()).Returns(Ok);

            _sut = new(_handlerCode);
        }

        [Fact]
        public async Task Handle_ShouldCallTheHandlerCodeWithTheGivenEventData()
        {
            await _sut.Handle(new(Value), default);

            _handlerCode.Received(1)(Value);
        }

        [Fact]
        public async Task Handle_ShouldNotCallTheHandlerCode_IfTheContextAlreadyContainsAnError()
        {
            var context = new EventContext<int>(Value);
            context.SetError(TestError.Create());

            await _sut.Handle(context, default);

            _handlerCode.DidNotReceiveWithAnyArgs()(default);
        }

        [Fact]
        public async Task Handle_ShouldSetAnErrorOnTheContext_IfTheHandlerCodeReturnsAnError()
        {
            var context = new EventContext<int>(Value);
            var error = TestError.Create();
            _handlerCode(Arg.Any<int>()).Returns(error);

            await _sut.Handle(context, default);

            context.Error.ShouldContain(error);
        }
    }
}
