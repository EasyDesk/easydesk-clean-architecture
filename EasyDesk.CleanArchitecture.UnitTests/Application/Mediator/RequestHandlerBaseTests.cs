using EasyDesk.CleanArchitecture.Application.Authorization;
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
            private readonly Func<Request, Response<int>> _resultProvider;
            private readonly Action<AuthorizationPolicyBuilder> _policyConfig;

            public Handler(Func<Request, Response<int>> resultProvider, Action<AuthorizationPolicyBuilder> policyConfig)
            {
                _resultProvider = resultProvider;
                _policyConfig = policyConfig;
            }

            protected override Task<Response<int>> Handle(Request request) => Task.FromResult(_resultProvider(request));

            protected override void AuthorizationPolicy(AuthorizationPolicyBuilder policy, Request request) => _policyConfig(policy);
        }

        private readonly Handler _sut;
        private readonly Request _request = new();
        private readonly Func<Request, Response<int>> _resultProvider;
        private readonly Action<AuthorizationPolicyBuilder> _policyConfig;

        public RequestHandlerBaseTests()
        {
            _resultProvider = Substitute.For<Func<Request, Response<int>>>();
            _resultProvider(_request).Returns(10);

            _policyConfig = Substitute.For<Action<AuthorizationPolicyBuilder>>();

            _sut = new(_resultProvider, _policyConfig);
        }

        [Fact]
        public async Task Handle_ShouldReturnForbidden_IfRequestingUserIsNotAuthorized()
        {
            _policyConfig(Arg.Do<AuthorizationPolicyBuilder>(policy => policy.Fail()));
            
            var result = await _sut.Handle(new(_request, Substitute.For<IUserInfo>()), default);

            result.ShouldBe(Errors.Forbidden());
            _resultProvider.DidNotReceiveWithAnyArgs()(default);
        }

        [Theory]
        [MemberData(nameof(Responses))]
        public async Task Handle_ShouldDelegateToTheInnerHandle_IfRequestingUserIsAuthorized(
            Response<int> expected)
        {
            _resultProvider(_request).Returns(expected);

            var result = await _sut.Handle(new(_request, Substitute.For<IUserInfo>()), default);

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
