namespace EasyDesk.CleanArchitecture.IntegrationTests;

[CollectionDefinition(nameof(SampleApplicationTestCollection))]
public class SampleApplicationTestCollection :
    ICollectionFixture<SampleAppTestsFixture>
{
}
