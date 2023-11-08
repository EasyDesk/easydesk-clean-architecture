using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using System.Runtime.CompilerServices;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;

public static class HttpResponseWrapperExtensions
{
    public static async Task<VerifiableHttpResponse<T, M>> Verify<T, M>(this HttpResponseWrapper<T, M> response, Action<SettingsTask>? configureVerifySettings = null, [CallerFilePath] string sourceFile = "")
    {
        var result = await response.AsVerifiable();
        await Verifier.Verify(result, sourceFile: sourceFile).Also(configureVerifySettings);
        return result;
    }
}
