using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.UserManuals.Models;

public partial record AddProductToUserManualModel : BaseNopModel
{
    public AddProductToUserManualModel()
    {
    }

    public int UserManualId { get; set; }

    public IList<int> SelectedProductIds { get; set; } = [];
}