using System;

namespace EasyDesk.CleanArchitecture.Application.Authorization
{
    public static class AuthorizationPolicies
    {
        public static AuthorizationPolicy Create(Action<AuthorizationPolicyBuilder> config)
        {
            var builder = new AuthorizationPolicyBuilder();
            config(builder);
            return builder.Build();
        }
    }
}
