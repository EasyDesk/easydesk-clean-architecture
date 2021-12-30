using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.Tools.Collections;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Web.Startup
{
    public partial class BaseStartup
    {
        private IImmutableList<IAuthenticationScheme> _schemes;

        private void AddAuthentication(IServiceCollection services)
        {
            var authOptions = new AuthenticationOptions();
            SetupAuthentication(authOptions);
            _schemes = authOptions.Schemes;
            if (_schemes.Count == 0)
            {
                return;
            }

            var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = _schemes[0].Name;
                options.DefaultChallengeScheme = _schemes[0].Name;
            });
            authOptions.Schemes.ForEach(scheme =>
            {
                scheme.AddUtilityServices(services);
                scheme.AddAuthenticationHandler(authBuilder);
            });
        }

        protected virtual void SetupAuthentication(AuthenticationOptions options)
        {
        }
    }

    public class AuthenticationOptions
    {
        public IImmutableList<IAuthenticationScheme> Schemes { get; private set; } = ImmutableList<IAuthenticationScheme>.Empty;

        public AuthenticationOptions AddScheme(IAuthenticationScheme scheme)
        {
            Schemes = Schemes.Add(scheme);
            return this;
        }
    }
}
