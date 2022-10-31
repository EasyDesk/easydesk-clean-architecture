using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public record KnownMessageTypes(IImmutableSet<Type> Types);
