using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Seeding;

public class SeedHolder<TSeed>
{
    private Option<TSeed> _currentSeed = None;

    public TSeed Seed => _currentSeed.OrElseThrow(() => new InvalidOperationException("Accessing seed outside of its lifetime."));

    public void StoreSeed(TSeed seed)
    {
        _currentSeed = Some(seed);
    }

    public void ClearSeed()
    {
        _currentSeed = None;
    }
}
