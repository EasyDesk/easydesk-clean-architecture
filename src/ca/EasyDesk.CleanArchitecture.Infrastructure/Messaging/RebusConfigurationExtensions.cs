using NodaTime;
using Rebus.Config;
using Rebus.Serialization;
using Rebus.Serialization.Json;
using Rebus.Time;
using Rebus.Topic;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public static class RebusConfigurationExtensions
{
    public static RebusConfigurer UseJsonOptions(this RebusConfigurer configurer, JsonSerializerOptions options)
    {
        return configurer.Serialization(s => s.UseSystemTextJson(options));
    }

    public static RebusConfigurer UseNodaTimeClock(this RebusConfigurer configurer, IClock clock)
    {
        return configurer.Options(o => o.Register<IRebusTime>(_ => new NodaTimeRebusClock(clock)));
    }

    public static RebusConfigurer UseKnownMessageTypesConventions(this RebusConfigurer configurer, IEnumerable<Type> messageTypes)
    {
        return configurer.Options(o =>
        {
            o.Register(_ => new KnownTypesConvention(messageTypes));
            o.Register<ITopicNameConvention>(c => c.Get<KnownTypesConvention>());
            o.Register<IMessageTypeNameConvention>(c => c.Get<KnownTypesConvention>());
        });
    }
}
