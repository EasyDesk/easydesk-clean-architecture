namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public abstract class CancellationTokenProvider
{
    public abstract CancellationToken Token { get; }

    public static implicit operator CancellationToken(CancellationTokenProvider provider) =>
        provider.Token;

    public static implicit operator CancellationTokenProvider(CancellationToken cancellationToken) =>
        new CancellationTokenWrapper(cancellationToken);
}
