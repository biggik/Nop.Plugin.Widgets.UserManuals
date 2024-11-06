using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.UserManuals.Models;

public partial record UserManualProductSearchModel : BaseSearchModel
{
    public int UserManualId { get; set; }
}
