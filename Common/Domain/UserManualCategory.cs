using Nop.Core;

namespace Nop.Plugin.Widgets.UserManuals.Domain;

public partial class UserManualCategory : BaseEntity
{
    public string Name { get; set; }

    public int DisplayOrder { get; set; }

    public bool Published { get; set; }
}