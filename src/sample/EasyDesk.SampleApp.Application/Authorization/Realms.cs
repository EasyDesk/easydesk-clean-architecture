using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.SampleApp.Application.Authorization;

public static class Realms
{
    public static Realm MainRealm => Realm.Default;

    public static Identity MainIdentity(this Agent agent) =>
        agent.RequireIdentity(MainRealm);
}
