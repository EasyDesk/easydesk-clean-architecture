using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Application.UserInfo;
using EasyDesk.CleanArchitecture.Domain.Model.Roles;
using EasyDesk.Testing.Utils;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Mediator
{
    public class RequestHandlerBaseTests
    {
        private static readonly Role _role = Role.From("Role");

        private record Request : RequestBase<int>;

        private class Handler : RequestHandlerBase<Request, int>
        {
            private readonly Func<Request, Response<int>> _result;

            public Handler(Func<Request, Response<int>> result)
            {
                _result = result;
            }

            protected override Task<Response<int>> Handle(Request request) => Task.FromResult(_result(request));

            protected override bool IsAuthorized(Request request, IUserInfo userInfo) => userInfo.HasRole(_role);
        }

        private readonly Handler _sut;
        private readonly Request _request = new();
        private readonly IUserInfo _userInfo;
        private readonly Func<Request, Response<int>> _resultProvider;

        public RequestHandlerBaseTests()
        {
            _userInfo = Substitute.For<IUserInfo>();

            _resultProvider = Substitute.For<Func<Request, Response<int>>>();
            _resultProvider(_request).Returns(10);

            _sut = new(_resultProvider);
        }

        [Fact]
        public async Task Handle_ShouldReturnForbidden_IfRequestingUserIsNotAuthorized()
        {
            _userInfo.HasRole(_role).Returns(false);

            var result = await _sut.Handle(new(_request, _userInfo), default);

            result.ShouldBe(Errors.Forbidden());
            _resultProvider.DidNotReceiveWithAnyArgs()(default);
        }

        [Theory]
        [MemberData(nameof(Responses))]
        public async Task Handle_ShouldDelegateToTheInnerHandle_IfRequestingUserIsAuthorized(
            Response<int> expected)
        {
            _resultProvider(_request).Returns(expected);
            _userInfo.HasRole(_role).Returns(true);

            var result = await _sut.Handle(new(_request, _userInfo), default);

            result.ShouldBe(expected);
            _resultProvider.Received(1)(_request);
        }

        public static IEnumerable<object[]> Responses()
        {
            yield return new object[] { Success(10) };
            yield return new object[] { Failure<int>(TestError.Create()) };
        }
    }
}
