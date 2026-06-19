using Autofac;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;
using EasyDesk.CleanArchitecture.Testing.Integration.OpenApi;
using EasyDesk.Commons.Collections;
using EasyDesk.Testing.VerifyConfiguration;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using static EasyDesk.SampleApp.Web.Controllers.V_1_0.Test.TestController;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public class ApiTests : SampleAppIntegrationTest
{
    public ApiTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ShouldSerializeAndSerializeNodatime()
    {
        await Session
            .Http
            .TestNodaTimeSerialization(new())
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldGenerateOpenApiDocuments()
    {
        var openApiHelper = Session.LifetimeScope.Resolve<OpenApiTestHelper>();

        foreach (var documentKey in openApiHelper.DocumentKeys)
        {
            var openApiProvider = Session.Host.LifetimeScope.ResolveKeyed<IOpenApiDocumentProvider>(documentKey);
            var doc = await openApiProvider.GetOpenApiDocumentAsync(TestContext.Current.CancellationToken);
            await using (var stream = new MemoryStream())
            {
                await doc.SerializeAsJsonAsync(stream, OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
            }
            await using (var stream = new MemoryStream())
            {
                await doc.SerializeAsYamlAsync(stream, OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
            }
        }
    }

    [Fact]
    public async Task ShouldProvideOpenApiDocumentsThroughHttp()
    {
        var openApiHelper = Session.LifetimeScope.Resolve<OpenApiTestHelper>();

        foreach (var documentKey in openApiHelper.DocumentKeys)
        {
            var document = await openApiHelper.GetOpenApiDocument(documentKey);
            await Verify(document).UseNamedParameter(documentKey);
        }
    }

    [Fact]
    public async Task ShouldProvideSwagger()
    {
        var openApiHelper = Session.LifetimeScope.Resolve<OpenApiTestHelper>();

        var swaggerPage = await openApiHelper.GetSwaggerPage();
        await Verify(swaggerPage);
    }

    [Fact]
    public async Task ShouldSerializeAndDeserializeFixedMapsStringObject()
    {
        await Session
            .Http
            .TestFixedMapStringObject(ImmutableCollections.Map<string, object>(("a", 5), ("b", "e")))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldSerializeAndDeserializeFixedMapsStringRecord()
    {
        await Session
            .Http
            .TestFixedMapStringRecord(ImmutableCollections.Map(("a", new TestRecord("aa", 2))))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldSerializeAndDeserializeFixedMapsRecordRecord()
    {
        await Session
            .Http
            .TestFixedMapRecordRecord(ImmutableCollections.Map((new TestRecord("ee", 5), new TestRecord("aa", 2))))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldSerializeAndDeserializedFixedMapEnumRecord()
    {
        await Session
            .Http
            .TestFixedMapEnumRecord(ImmutableCollections.Map((EnumType.C, new TestRecord("cc", 2)), (EnumType.B, new TestRecord("bb", 3))))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldSerializeAndDeserializeDictionaryEnumRecord()
    {
        await Session
            .Http
            .TestDictionaryEnumRecord(new Dictionary<EnumType, TestRecord> { [EnumType.C] = new("cc", 2), [EnumType.B] = new("bb", 3), })
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldSerializeFixedMapEnumKeys()
    {
        var response = await Session
            .Http
            .TestFixedMapEnumRecord(ImmutableCollections.Map((EnumType.C, new TestRecord("cc", 2)), (EnumType.B, new TestRecord("bb", 3))))
            .Send()
            .GetResponse();

        await Verify(response.Content.AsString());
    }

    public static TheoryData<string?> Guids() =>
    [
        Guid.Empty.ToString(),
        string.Empty,
        "not-a-guid",
        null!,
    ];

    [Theory]
    [MemberData(nameof(Guids))]
    public async Task ShouldHandleGuidInQuery(string? value)
    {
        await Session
            .Http
            .TestGuidInQuery(value)
            .Send()
            .Verify(c => c.UseParameters(value));
    }

    [Theory]
    [MemberData(nameof(Guids))]
    public async Task ShouldHandleGuidInBody(string? value)
    {
        await Session
            .Http
            .TestGuidInBody(value)
            .Send()
            .Verify(c => c.UseParameters(value));
    }

    [Theory]
    [MemberData(nameof(Guids))]
    public async Task ShouldHandleGuidInRoute(string? value)
    {
        await Session
            .Http
            .TestGuidInRoute(value)
            .Send()
            .Verify(c => c.UseParameters(value));
    }
}
