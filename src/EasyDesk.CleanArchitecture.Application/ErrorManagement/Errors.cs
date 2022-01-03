using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static EasyDesk.Tools.Collections.ImmutableCollections;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public static class Errors
{
    public static class Codes
    {
        public const string Internal = "InternalError";

        public const string Forbidden = "Forbidden";

        public const string NotFound = "NotFound";

        public const string PropertyValidationError = "PropertyValidationError";
    }

    private const string FormatPadding = "#";

    public static Error Internal(Exception ex) => new InternalError(ex);

    public static Error Forbidden(string message = null) => new ForbiddenError(message ?? "Not authorized");

    public static Error NotFound(Type entityType) => new NotFoundError(entityType);

    public static Error NotFound<T>() => NotFound(typeof(T));

    public static Error InvalidProperty(string propertyName, string errorMessage) =>
        new PropertyValidationError(propertyName, errorMessage);

    public static Error FromDomain(DomainError domainError) => new DomainErrorWrapper(domainError);

    public static Error Generic(string errorCode, string message, params object[] args)
    {
        var paramMap = new Dictionary<string, object>();
        var argsQueue = new Queue<object>(args);

        var formattedMessage = Regex.Replace(
            message,
            @"\\?{(.+?)}",
            match => CalculateMatchReplacement(match, paramMap, argsQueue));

        return new GenericError(formattedMessage, errorCode, Map(paramMap));
    }

    private static string CalculateMatchReplacement(Match match, Dictionary<string, object> paramMap, Queue<object> argsQueue)
    {
        if (match.Value.StartsWith(@"\"))
        {
            return match.Value[1..];
        }

        var contentInsideBraces = GetGroup(match, 1);

        var (name, format) = ParseContentInsideBraces(contentInsideBraces);
        var value = paramMap.GetOrAdd(name, () => argsQueue.Dequeue());

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
