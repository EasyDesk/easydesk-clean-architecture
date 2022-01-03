using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Shouldly;
using System.IO;
using System.Text;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Configuration;

public static class ConfigurationAssertions
{
    public static void ShouldBe(this IConfiguration actual, IConfiguration expected)
    {
        actual.AsEnumerable().ShouldBe(expected.AsEnumerable(), ignoreOrder: true);
    }
}

public class ConfigurationExtensionsTests
{
    private const string JsonConfiguration = @"
            {
                ""A"": {
                    ""AA"": 1,
                    ""AB"": 2,
                },
                ""B"": {
                    ""BA"": 3.2,
                },
                ""C"": ""Hello""
            }";

    private readonly IConfiguration _configuration;

    public ConfigurationExtensionsTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(JsonConfiguration)))
            .Build();
    }

    [Fact]
    public void RequireSection_ShouldReturnASection_IfItExists()
    {
        _configuration.RequireSection("A").ShouldBe(_configuration.GetSection("A"));
    }

    [Fact]
    public void RequireSection_ShouldReturnASection_IfPathExists()
    {
        _configuration.RequireSection("A:AA").ShouldBe(_configuration.GetSection("A:AA"));
    }

    [Fact]
    public void RequireSection_ShouldFail_IfTheSectionDoesNotExist()
    {
        var exception = Should.Throw<MissingConfigurationException>(() => _configuration.RequireSection("D"));
        exception.Key.ShouldBe("D");
    }

    [Fact]
    public void RequireSection_ShouldFail_IfThePathIsPartiallyWrong()
    {
        var exception = Should.Throw<MissingConfigurationException>(() => _configuration.RequireSection("B:BB"));
        exception.Key.ShouldBe("B:BB");
    }

    [Fact]
    public void RequireSection_ShouldReturnTheCorrectSubsection_WhenCalledOnASubsection()
    {
        var subsection = _configuration.RequireSection("B");
        subsection.RequireSection("BA").ShouldBe(_configuration.GetSection("B:BA"));
    }

    [Fact]
    public void RequireSection_ShouldFailWithTheCompletePathAsKey_WhenCalledOnASubsection()
    {
        var subsection = _configuration.RequireSection("B");
        var exception = Should.Throw<MissingConfigurationException>(() => subsection.RequireSection("BB"));
        exception.Key.ShouldBe("B:BB");
    }
}
