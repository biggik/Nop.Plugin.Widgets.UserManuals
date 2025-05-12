using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace Nop.Plugin.Widgets.UserManuals.Security;

public partial class UserManualPermissionConfigs
{
    public const string MANAGE_USER_MANUALS = "ManageUserManuals";

    internal static readonly PermissionConfig ManageEmployees = new(
        "Admin area. Manage User Manuals",
        MANAGE_USER_MANUALS,
        "UserManuals",
        nameof(StandardPermission.System),
        NopCustomerDefaults.AdministratorsRoleName);
}
