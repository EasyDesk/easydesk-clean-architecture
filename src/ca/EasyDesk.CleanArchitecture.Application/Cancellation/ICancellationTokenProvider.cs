namespace EasyDesk.CleanArchitecture.Application.Cancellation;

public interface ICancellationTokenProvider
{
    CancellationToken CancellationToken { get; }
}
