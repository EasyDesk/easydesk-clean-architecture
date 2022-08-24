using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public record KnownMessageTypes(IImmutableSet<Type> Types);
