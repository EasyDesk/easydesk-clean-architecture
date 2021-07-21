using EasyDesk.CleanArchitecture.Application.UserInfo;
using EasyDesk.CleanArchitecture.Domain.Model.Roles;
using System;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Application.Authorization
{
    public delegate bool AuthorizationPolicy(IUserInfo userInfo);

    public class AuthorizationPolicyBuilder
    {
        private AuthorizationPolicy _policy = _ => true;

        public AuthorizationPolicyBuilder Require(AuthorizationPolicy policy)
        {
            var current = _policy;
            _policy = user => current(user) && policy(user);
            return this;
        }

        public AuthorizationPolicyBuilder Either(params Action<AuthorizationPolicyBuilder>[] policiesConfigs)
        {
            var policies = policiesConfigs.Select(AuthorizationPolicies.Create);
            return Require(user => policies.Any(p => p(user)));
        }

        public AuthorizationPolicyBuilder RequireLogin() => Require(user => user.IsLoggedIn);

        public AuthorizationPolicyBuilder RequireRole(params Role[] roles) => Require(user => user.IsLoggedIn && user.Roles.Intersect(roles).Any());

        public AuthorizationPolicyBuilder RequireUserId(params Guid[] userIds) => Require(user => user.IsLoggedIn && userIds.Contains(user.UserId));

        public AuthorizationPolicyBuilder Fail() => Require(_ => false);

        public AuthorizationPolicy Build() => _policy;
    }
}
