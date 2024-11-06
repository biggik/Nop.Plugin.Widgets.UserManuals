using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Widgets.UserManuals.Resources;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.UserManuals.Models;

public partial record ConfigurationModel : BaseNopModel
{
    public ConfigurationModel()
    {
    }

    [NopResourceDisplayName(ConfigurationResources.WidgetZones)]
    public IList<int> WidgetZones { get; set; }

    public IList<SelectListItem> AvailableWidgetZones { get; set; }
}