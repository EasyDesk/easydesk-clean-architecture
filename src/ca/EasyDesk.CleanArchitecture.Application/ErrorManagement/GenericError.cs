﻿using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public partial record GenericError(string Code, string Message, IFixedMap<string, object> Parameters) : Error
{
    public static GenericError Create(string code, string message, params object[] args)
    {
        var paramMap = new Dictionary<string, object>();
        var argsQueue = new Queue<object>(args);

        var formattedMessage = GenericErrorParameterRegex.Instance().Replace(message, match => CalculateMatchReplacement(match, paramMap, argsQueue));

        return new(code, formattedMessage, paramMap.ToFixedSortedMap());
    }

    private static string CalculateMatchReplacement(Match match, Dictionary<string, object> paramMap, Queue<object> argsQueue)
    {
        if (match.Value.StartsWith('\\'))
        {
            return match.Value[1..];
        }

        var contentInsideBraces = GetGroup(match, 1);

        var (name, format) = ParseContentInsideBraces(contentInsideBraces);
        var value = paramMap.GetOrAdd(name, argsQueue.Dequeue);

        var formatString = format.Match(
            some: f => $"{{0:{f}}}",
            none: () => "{0}");

        return string.Format(formatString, value);
    }

    private static (string Name, Option<string> Format) ParseContentInsideBraces(string occurence)
    {
        var splits = occurence.Split(":", 2);
        return (splits[0], splits.Length == 2 ? Some(splits[1]) : None);
    }

    private static string GetGroup(Match match, int groupIndex) => match.Groups[groupIndex].Value;
}

public static partial class GenericErrorParameterRegex
{
    [GeneratedRegex("\\\\?{(.+?)}")]
    public static partial Regex Instance();
}
