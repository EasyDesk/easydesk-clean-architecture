using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using EasyDesk.Extensions.Configuration;
using Rebus.Retry.FailFast;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

internal class FailFastChecker : IFailFastChecker
{
    private readonly IFailFastChecker _fallback;
    private readonly IEnumerable<Func<Exception, Option<bool>>> _predicates;

    public FailFastChecker(IFailFastChecker fallback, IEnumerable<Func<Exception, Option<bool>>> predicates)
    {
        _fallback = fallback;
        _predicates = predicates;
    }

    public bool ShouldFailFast(string messageId, Exception exception) =>
        _predicates.SelectMany(p => p(exception)).FirstOption().Contains(true) || exception switch
        {
            // System
            DllNotFoundException
                or MemberAccessException
                or NotImplementedException
                or NotSupportedException => true,

            // Framework
            HandlerNotFoundException
                or MissingConfigurationException
                or RequiredModuleMissingException => true,

            _ => _fallback.ShouldFailFast(messageId, exception),
        };
}
