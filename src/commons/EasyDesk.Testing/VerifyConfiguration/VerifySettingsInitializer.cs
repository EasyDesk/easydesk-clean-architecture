using Argon;
using VerifyTests;

namespace EasyDesk.Testing.VerifyConfiguration;

public static class VerifySettingsInitializer
{
    public static void Initialize()
    {
        VerifyNewtonsoftJson.Enable();
        VerifierSettings.AddExtraSettings(settings =>
        {
            settings.Converters.Add(new OptionConverter());
            settings.Converters.Add(new NoneOptionConverter());
            settings.Converters.Add(new ErrorConverter());
            settings.NullValueHandling = NullValueHandling.Include;
            settings.DefaultValueHandling = DefaultValueHandling.Include;
        });

        VerifierSettings.DontIgnoreEmptyCollections();

        VerifyNodaTime.Enable();
    }
}
