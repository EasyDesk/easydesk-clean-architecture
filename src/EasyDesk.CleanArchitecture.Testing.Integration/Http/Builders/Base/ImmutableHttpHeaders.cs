﻿using EasyDesk.Tools.Collections;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public record class ImmutableHttpHeaders(
    IImmutableDictionary<string, IEnumerable<string>> Dictionary)
{
    public ImmutableHttpHeaders Replace(string header, string value) =>
        new(Dictionary
            .SetItem(
                header,
                EnumerableUtils.Items(value)));

    public ImmutableHttpHeaders Add(string header, string value) =>
        new(Dictionary
            .SetItem(
                header,
                Dictionary
                    .GetOption(header)
                    .OrElseGet(() => Enumerable.Empty<string>())
                    .Append(value)));

    public ImmutableHttpHeaders Remove(string header) =>
        new(Dictionary.Remove(header));

    public override string ToString() => Dictionary.Select(pair => $"{pair.Key}: {pair.Value.ConcatStrings(", ")}").ConcatStrings("\n");
}