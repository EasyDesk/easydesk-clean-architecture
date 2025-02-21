using EasyDesk.CleanArchitecture.Application.Cancellation;
using Rebus.Extensions;

namespace EasyDesk.CleanArchitecture.Infrastructure.Context;

internal class ContextCancellationTokenProvider : ICancellationTokenProvider
{
    private readonly ContextDetector _contextDetector;

    public ContextCancellationTokenProvider(ContextDetector contextDetector)
    {
        _contextDetector = contextDetector;
    }

    public CancellationToken CancellationToken => _contextDetector.MatchContext(
        httpContext: c => c.RequestAborted,
        messageContext: c => c.GetCancellationToken(),
        other: () => default);
}
