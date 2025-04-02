using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor;

public interface ITestHostBuilder
{
    ITestHostBuilder ConfigureHost(Action<IHostBuilder> configure);

    ITestHostBuilder ConfigureWebHost(Action<IWebHostBuilder> configure);

    ITestHostBuilder WithConfiguration(Action<IDictionary<string, string?>> configure);
}

public static class ITestHostBuilderExtensions
{
    public static ITestHostBuilder WithEnvironment(this ITestHostBuilder builder, string environment) =>
        builder.ConfigureWebHost(builder => builder.UseEnvironment(environment));

    public static ITestHostBuilder WithConfiguration(this ITestHostBuilder builder, string key, string value) =>
        builder.WithConfiguration(d => d.Add(key, value));
}
