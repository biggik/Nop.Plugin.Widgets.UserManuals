using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Nop.Plugin.Widgets.UserManuals.Models;
using Nop.Plugin.Widgets.UserManuals.Services;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.UserManuals.Components;

[ViewComponent(Name = "WidgetsProductUserManuals")]
public class WidgetsProductUserManualsViewComponent : NopViewComponent
{
    private readonly IUserManualService _userManualService;

    public WidgetsProductUserManualsViewComponent(IUserManualService userManualService)
    {
        _userManualService = userManualService;
    }

    public async Task<IViewComponentResult> InvokeAsync(RouteValueDictionary values)
    {
        var product = (ProductDetailsModel)values["additionalData"];
        var manuals = await _userManualService.GetByProductIdAsync(product.Id);
        return manuals != null && manuals.Any()
            ? View("~/Plugins/Widgets.UserManuals/Views/Shared/Components/WidgetProductUserManuals/Default.cshtml",
                await (from manual in manuals
                 select manual.ToModel()
                ).ToListAsync())
            : Content("");
    }
}
