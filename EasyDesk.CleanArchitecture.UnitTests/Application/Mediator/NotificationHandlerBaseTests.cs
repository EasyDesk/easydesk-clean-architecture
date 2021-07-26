using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using NSubstitute;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;
using EasyDesk.CleanArchitecture.Testing;

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

        private const int _value = 10;
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
            await _sut.Handle(new(_value), default);

            _handlerCode.Received(1)(_value);
        }

        [Fact]
        public async Task Handle_ShouldNotCallTheHandlerCode_IfTheContextAlreadyContainsAnError()
        {
            var context = new EventContext<int>(_value);
            context.SetError(TestError.Create());

            await _sut.Handle(context, default);

            _handlerCode.DidNotReceiveWithAnyArgs()(default);
        }

        [Fact]
        public async Task Handle_ShouldSetAnErrorOnTheContext_IfTheHandlerCodeReturnsAnError()
        {
            var context = new EventContext<int>(_value);
            var error = TestError.Create();
            _handlerCode(Arg.Any<int>()).Returns(error);

            await _sut.Handle(context, default);

            context.Error.ShouldContain(error);
        }
    }
}
