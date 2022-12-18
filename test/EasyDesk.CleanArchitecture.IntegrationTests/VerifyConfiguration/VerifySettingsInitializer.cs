using System.Runtime.CompilerServices;

namespace EasyDesk.CleanArchitecture.IntegrationTests.VerifyConfiguration;

public static class VerifySettingsInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.AddExtraSettings(settings =>
        {
            settings.Converters.Add(new OptionConverter());
        });
    }
}
