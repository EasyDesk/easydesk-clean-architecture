using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Rebus.Config;
using Rebus.Serialization;
using Rebus.Serialization.Json;
using Rebus.Time;
using Rebus.Topic;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public static class RebusDefaults
{
    public static RebusConfigurer ConfigureStandardBehavior(
        this RebusConfigurer configurer,
        RebusEndpoint endpoint,
        RebusMessagingOptions options,
        IServiceProvider serviceProvider)
    {
        options.ApplyDefaultConfiguration(endpoint, configurer);

        configurer.Routing(r =>
        {
            r.Register(_ => new OutgoingCommandRouter(endpoint));
        });

        configurer.Logging(l =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            l.MicrosoftExtensionsLogging(loggerFactory);
        });

        configurer.Serialization(s =>
        {
            var jsonSettings = serviceProvider.GetRequiredService<JsonSettingsConfigurator>();
            s.UseNewtonsoftJson(jsonSettings.CreateSettings());
        });

        configurer.Options(o =>
        {
            var knownMessageTypes = serviceProvider.GetRequiredService<KnownMessageTypes>();
            var clock = serviceProvider.GetRequiredService<IClock>();
            o.Decorate<IRebusTime>(_ => new NodaTimeRebusClock(clock));
            o.Decorate<ITopicNameConvention>(_ => new TopicNameConvention());
            o.Decorate<IMessageTypeNameConvention>(c => new KnownTypesConvention(knownMessageTypes));
            o.LogPipeline(verbose: true);
        });

        return configurer;
    }
}
