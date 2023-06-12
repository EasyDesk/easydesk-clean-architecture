using EasyDesk.CleanArchitecture.Application.Authorization;

namespace EasyDesk.SampleApp.Application.Authorization;

public static class PermissionSettings
{
    public static void RolesToPermissions(StaticRolesToPermissionsBuilder builder)
    {
        builder.ForRoles(Roles.Admin)
            .AddPermissions(
                Permissions.CanEditPeople,
                Permissions.CanEditPets);
    }
}
