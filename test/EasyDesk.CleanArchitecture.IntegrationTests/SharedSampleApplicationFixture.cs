namespace EasyDesk.CleanArchitecture.IntegrationTests;

[CollectionDefinition(nameof(SharedSampleApplicationFixture))]
public class SharedSampleApplicationFixture :
    ICollectionFixture<SampleAppTestsFixture>
{
}
