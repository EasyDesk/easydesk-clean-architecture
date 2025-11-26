using Autofac;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;
using EasyDesk.CleanArchitecture.Testing.Integration.OpenApi;
using EasyDesk.Commons.Collections;
using EasyDesk.Testing.VerifyConfiguration;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
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
        var swaggerProvider = Session.Host.LifetimeScope.Resolve<ISwaggerProvider>();

        foreach (var documentKey in openApiHelper.DocumentKeys)
        {
            var doc = swaggerProvider.GetSwagger(documentKey);
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
    public async Task ShouldSerializeAndDeserializeFixedMapsStringObject()
    {
        var req = Session
            .Http
            .TestFixedMapStringObject(ImmutableCollections.Map<string, object>(("a", 5), ("b", "e")))
            .Send();

        await Verify((await req.GetResponse()).Content.AsString())
            .UseTextForParameters("raw");

        await Verify(await req.AsData())
            .UseTextForParameters("deserialized");
    }

    [Fact]
    public async Task ShouldSerializeAndDeserializeFixedMapsStringRecord()
    {
        var req = Session
            .Http
            .TestFixedMapStringRecord(ImmutableCollections.Map(("a", new TestRecord("aa", 2))))
            .Send();

        await Verify((await req.GetResponse()).Content.AsString())
            .UseTextForParameters("raw");

        await Verify(await req.AsData())
            .UseTextForParameters("deserialized");
    }

    [Fact]
    public async Task ShouldSerializeAndDeserializeFixedMapsRecordRecord()
    {
        var req = Session
            .Http
            .TestFixedMapRecordRecord(ImmutableCollections.Map((new TestRecord("ee", 5), new TestRecord("aa", 2))))
            .Send();

        await Verify((await req.GetResponse()).Content.AsString())
            .UseTextForParameters("raw");

        await Verify(await req.AsData())
            .UseTextForParameters("deserialized");
    }
}
