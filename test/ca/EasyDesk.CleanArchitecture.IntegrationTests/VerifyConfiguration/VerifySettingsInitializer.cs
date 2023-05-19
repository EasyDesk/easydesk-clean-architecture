using EasyDesk.SampleApp.Application.Dto;
using EasyDesk.Testing.VerifyConfiguration;
using System.Runtime.CompilerServices;

namespace EasyDesk.CleanArchitecture.IntegrationTests.VerifyConfiguration;

public static class VerifySettings
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySettingsInitializer.Initialize();

        VerifierSettings.ScrubMember<PetDto>(p => p.Id);
    }
}
