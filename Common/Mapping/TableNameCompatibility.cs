using Nop.Data.Mapping;
using Nop.Plugin.Widgets.UserManuals.Domain;

namespace Nop.Plugin.Widgets.UserManuals.Mapping;

public partial class TableNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new()
    {
        [typeof(UserManualProduct)] = "StatusUserManualProduct",
        [typeof(UserManualCategory)] = "StatusUserManualCategory",
        [typeof(UserManual)] = "StatusUserManual"
    };

    public Dictionary<(Type, string), string> ColumnName => [];
}
