using System.Collections.Generic;
using Nop.Services.Security;
using Nop.Core.Domain.Security;
using System.Linq;

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

#if NOP_PRE_4_3
        private static readonly DefaultPermissionRecord defaultPermission = new DefaultPermissionRecord { CustomerRoleSystemName = "Administrators" };
        public virtual IEnumerable<DefaultPermissionRecord> GetDefaultPermissions() =>
            new[] { defaultPermission };
#else
        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions() =>
            return new HashSet<(string, PermissionRecord[])> { ("Administrators", new PermissionRecord[0]) };
#endif
    }
}
