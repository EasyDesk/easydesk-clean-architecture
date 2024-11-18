using Argon;

namespace EasyDesk.Testing.VerifyConfiguration;

public static class VerifySettingsInitializer
{
    public static void Initialize()
    {
        VerifySystemJson.Initialize();
        VerifierSettings.AddExtraSettings(settings =>
        {
            settings.Converters.Add(new OptionConverter());
            settings.Converters.Add(new NoneOptionConverter());
            settings.Converters.Add(new ErrorConverter());
            settings.Converters.Add(new FixedListConverter());
            settings.Converters.Add(new FixedSetConverter());
            settings.Converters.Add(new FixedMapConverter());
            settings.NullValueHandling = NullValueHandling.Include;
            settings.DefaultValueHandling = DefaultValueHandling.Include;
        });

        VerifierSettings.DontIgnoreEmptyCollections();

        VerifyNodaTime.Initialize();
    }
}
