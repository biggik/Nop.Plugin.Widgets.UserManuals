using Nop.Plugin.Widgets.UserManuals.Resources;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.UserManuals.Models;

public partial record CategoryModel : BaseNopEntityModel
{
    [NopResourceDisplayName(CategoryResources.Name)]
    public string Name { get; set; }

    [NopResourceDisplayName(GenericResources.DisplayOrder)]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName(GenericResources.Published)]
    public bool Published { get; set; } = true;
}