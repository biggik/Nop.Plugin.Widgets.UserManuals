using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.UserManuals.Services
{
    public partial class UserManualPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageUserManuals = new PermissionRecord { Name = "Admin area. Manage User Manuals", SystemName = nameof(ManageUserManuals), Category = "UserManuals" };

        public virtual IEnumerable<PermissionRecord> GetPermissions() =>
            new[]
            {
                ManageUserManuals
            };

        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions() =>
            new()
            { ("Administrators", new[] { ManageUserManuals }) };
    }
}
