using EasyDesk.Testing.VerifyConfiguration;
using System.Runtime.CompilerServices;

namespace EasyDesk.CleanArchitecture.UnitTests.VerifyConfiguration;

public static class VerifySettings
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySettingsInitializer.Initialize();
    }
}
