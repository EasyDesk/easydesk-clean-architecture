using Rebus.Config;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public delegate void RebusTransportConfiguration(StandardConfigurer<ITransport> configurer, RebusEndpoint endpoint);
