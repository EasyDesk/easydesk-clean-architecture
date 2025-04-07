using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Session;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Seeding;

public class SeedManager<TFixture, TSeed>
    where TFixture : IntegrationTestsFixture
{
    private readonly ISeeder<TFixture, TSeed> _seeder;
    private readonly TFixture _fixture;
    private Option<TSeed> _currentSeed = None;

    public SeedManager(ISeeder<TFixture, TSeed> seeder, TFixture fixture)
    {
        _seeder = seeder;
        _fixture = fixture;
    }

    public TSeed Seed => _currentSeed.OrElseThrow(() => new InvalidOperationException("Accessing seed outside of its lifetime."));

    public async Task ApplySeed()
    {
        await using var session = new IntegrationTestSession<TFixture>(_fixture, _seeder.ConfigureSession);
        var seed = await _seeder.Seed(session);
        _currentSeed = Some(seed);
    }

    public void ClearSeed()
    {
        _currentSeed = None;
    }
}
