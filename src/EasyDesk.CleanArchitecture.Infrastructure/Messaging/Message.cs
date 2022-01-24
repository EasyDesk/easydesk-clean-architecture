using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using System;
using System.Collections.Immutable;
using static EasyDesk.Tools.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public record Message(Guid Id, object Content, IImmutableDictionary<string, string> Metadata)
{
    public static Message CreateEmpty() => CreateEmpty(metadata: Map<string, string>());

    public static Message CreateEmpty(IImmutableDictionary<string, string> metadata) => Create(Nothing.Value, metadata);

    public static Message Create(object content) => Create(content, metadata: Map<string, string>());

    public static Message Create(object content, IImmutableDictionary<string, string> metadata) =>
        new(Id: Guid.NewGuid(), content, metadata);

    public Message WithMetadata(string key, string value) => this with { Metadata = Metadata.SetItem(key, value) };

    public Option<string> GetMetadata(string key) => Metadata.GetOption(key);
}
