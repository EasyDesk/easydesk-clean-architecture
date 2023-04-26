namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public class CancellationTokenWrapper : CancellationTokenProvider
{
    public CancellationTokenWrapper(CancellationToken cancellationToken)
    {
        Token = cancellationToken;
    }

    public override CancellationToken Token { get; }
}
