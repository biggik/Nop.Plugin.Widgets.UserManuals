using Nop.Services.Security;

namespace Nop.Plugin.Widgets.UserManuals.Security;

public partial class UserManualPermissionConfigProvider : IPermissionConfigManager
{
    public IList<PermissionConfig> AllConfigs
        => [UserManualPermissionConfigs.ManageEmployees];
}
