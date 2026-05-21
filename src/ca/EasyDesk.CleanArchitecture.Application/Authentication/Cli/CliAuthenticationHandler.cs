using EasyDesk.CleanArchitecture.Application.CommandLine;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Authentication.Cli;

public class CliAuthenticationHandler : IAuthenticationHandler
{
    private readonly CliContextAccessor _cliContextAccessor;
    private readonly CliAuthenticationDelegate _authenticationDelegate;

    public CliAuthenticationHandler(CliContextAccessor cliContextAccessor, CliAuthenticationDelegate authenticationDelegate)
    {
        _cliContextAccessor = cliContextAccessor;
        _authenticationDelegate = authenticationDelegate;
    }

    public async Task<Option<AuthenticationResult>> Authenticate()
    {
        return await _cliContextAccessor.CliContext
            .FlatMapAsync(_authenticationDelegate.Invoke);
    }
}
