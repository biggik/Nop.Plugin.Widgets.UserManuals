using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.UserManuals.Components;

[ViewComponent(Name = "WidgetsUserManuals")]
public class WidgetsUserManualsViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke()
        => View("~/Plugins/Widgets.UserManuals/Views/UserManuals/PublicInfo.cshtml");
}
