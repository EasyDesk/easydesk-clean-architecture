using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Shouldly;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Configuration
{
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
        public void GetRequiredSection_ShouldReturnASection_IfItExists()
        {
            _configuration.GetRequiredSection("A").ShouldBe(_configuration.GetSection("A"));
        }

        [Fact]
        public void GetRequiredSection_ShouldReturnASection_IfPathExists()
        {
            _configuration.GetRequiredSection("A:AA").ShouldBe(_configuration.GetSection("A:AA"));
        }

        [Fact]
        public void GetRequiredSection_ShouldFail_IfTheSectionDoesNotExist()
        {
            var exception = Should.Throw<MissingConfigurationException>(() => _configuration.GetRequiredSection("D"));
            exception.Key.ShouldBe("D");
        }

        [Fact]
        public void GetRequiredSection_ShouldFail_IfThePathIsPartiallyWrong()
        {
            var exception = Should.Throw<MissingConfigurationException>(() => _configuration.GetRequiredSection("B:BB"));
            exception.Key.ShouldBe("B:BB");
        }

        [Fact]
        public void GetRequiredSection_ShouldReturnTheCorrectSubsection_WhenCalledOnASubsection()
        {
            var subsection = _configuration.GetRequiredSection("B");
            subsection.GetRequiredSection("BA").ShouldBe(_configuration.GetSection("B:BA"));
        }

        [Fact]
        public void GetRequiredSection_ShouldFailWithTheCompletePathAsKey_WhenCalledOnASubsection()
        {
            var subsection = _configuration.GetRequiredSection("B");
            var exception = Should.Throw<MissingConfigurationException>(() => subsection.GetRequiredSection("BB"));
            exception.Key.ShouldBe("B:BB");
        }

        [Fact]
        public void GetRequiredSection_ShouldFail_IfTheKeyIsEmpty()
        {
            Should.Throw<ArgumentException>(() => _configuration.GetRequiredSection(string.Empty));
        }
    }
}
