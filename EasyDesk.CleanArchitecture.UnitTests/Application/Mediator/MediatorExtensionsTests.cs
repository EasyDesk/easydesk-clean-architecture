using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Application.UserInfo;
using EasyDesk.Testing.Utils;
using MediatR;
using NSubstitute;
using Shouldly;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Mediator
{
    public class MediatorExtensionsTests
    {
        private record Event;
        private record Request : RequestBase<int>;

        private readonly Event _event = new();
        private readonly Request _request = new();
        private readonly IMediator _mediator;

        public MediatorExtensionsTests()
        {
            _mediator = Substitute.For<IMediator>();
        }

        [Fact]
        public async Task PublishEvent_ShouldPublishTheGivenEventWrappedInAnEventContext()
        {
            await _mediator.PublishEvent(_event);

            await _mediator.Received(1).Publish(Arg.Is<object>(ctx => (ctx as EventContext<Event>).EventData == _event));
        }

        [Fact]
        public async Task PublishEvent_ShouldReturnSuccess_IfNoHandlerSetsAnError()
        {
            var result = await _mediator.PublishEvent(_event);

            result.ShouldBe(Ok);
        }

        [Fact]
        public async Task PublishEvent_ShouldReturnAnError_IfAnyHandlerSetsAnErrorOnTheContext()
        {
            var error = TestError.Create();
            await _mediator.Publish(Arg.Do<object>(ctx => (ctx as EventContext<Event>).SetError(error)));

            var result = await _mediator.PublishEvent(_event);

            result.ShouldBe(error);
        }

        [Fact]
        public async Task SendRequestWithContext_ShouldSendTheGivenRequestWrappedInARequestContext()
        {
            await _mediator.SendRequestWithContext(_request, Substitute.For<IUserInfo>());

            await _mediator.Received(1).Send(Arg.Is<object>(ctx => (ctx as RequestContext<Request, int>).Request == _request));
        }

        [Theory]
        [MemberData(nameof(Responses))]
        public async Task SendRequestWithContext_ShouldReturnTheResponseGivenByTheMediator(
            Response<int> expectedResponse)
        {
            _mediator.Send(default).ReturnsForAnyArgs(expectedResponse);

            var result = await _mediator.SendRequestWithContext(_request, Substitute.For<IUserInfo>());

            result.ShouldBe(expectedResponse);
        }

        public static IEnumerable<object[]> Responses()
        {
            yield return new object[] { Success(10) };
            yield return new object[] { Failure<int>(TestError.Create()) };
        }
    }
}
