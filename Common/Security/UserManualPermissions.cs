using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Widgets.UserManuals.Security;

public partial class UserManualPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageUserManuals = new() { Name = "Admin area. Manage User Manuals", SystemName = nameof(ManageUserManuals), Category = "UserManuals" };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
        => [ManageUserManuals];

    public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        => [("Administrators", new[] { ManageUserManuals })];
}
