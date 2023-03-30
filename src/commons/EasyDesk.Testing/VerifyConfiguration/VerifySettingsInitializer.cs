using Argon;

namespace EasyDesk.Testing.VerifyConfiguration;

public static class VerifySettingsInitializer
{
    public static void Initialize()
    {
        VerifyNewtonsoftJson.Initialize();
        VerifierSettings.AddExtraSettings(settings =>
        {
            settings.Converters.Add(new OptionConverter());
            settings.Converters.Add(new NoneOptionConverter());
            settings.Converters.Add(new ErrorConverter());
            settings.NullValueHandling = NullValueHandling.Include;
            settings.DefaultValueHandling = DefaultValueHandling.Include;
        });

        VerifierSettings.DontIgnoreEmptyCollections();

        VerifyNodaTime.Initialize();
    }
}
