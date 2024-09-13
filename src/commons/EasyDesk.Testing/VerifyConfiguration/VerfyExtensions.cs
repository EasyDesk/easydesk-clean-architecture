using EasyDesk.Commons.Collections;
using System.Runtime.CompilerServices;

namespace EasyDesk.Testing.VerifyConfiguration;

public static class VerfyExtensions
{
    public const string NamedParameterContextKey = "namedParameters";

    public static SettingsTask UseNamedParameter(this SettingsTask settings, object? parameter, [CallerArgumentExpression(nameof(parameter))] string name = "")
    {
        var parameters = settings.CurrentSettings.Context.GetOrAdd(NamedParameterContextKey, () => new Dictionary<string, object?>()) as Dictionary<string, object?>;
        parameters![name] = parameter;
        settings.UseTextForParameters(parameters.Select(kv => $"{kv.Key}={kv.Value}").ConcatStrings("_"));
        return settings;
    }
}
