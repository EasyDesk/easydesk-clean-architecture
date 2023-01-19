using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

namespace EasyDesk.SampleApp.Application.Authorization;

public static class PermissionSettings
{
    public static void RolesToPermissions(StaticRolesToPermissionsBuilder builder)
    {
        builder.ForRoles(Roles.Admin)
            .AddPermissions(
                Permissions.CAN_EDIT_PEOPLE,
                Permissions.CAN_EDIT_PETS);
    }
}
