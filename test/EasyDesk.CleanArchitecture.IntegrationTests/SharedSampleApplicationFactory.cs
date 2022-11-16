namespace EasyDesk.CleanArchitecture.IntegrationTests;

[CollectionDefinition(nameof(SharedSampleApplicationFactory))]
public class SharedSampleApplicationFactory :
    ICollectionFixture<SampleApplicationFactory>
{
}
