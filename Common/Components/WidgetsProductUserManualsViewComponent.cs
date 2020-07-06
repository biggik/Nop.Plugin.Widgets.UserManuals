using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Nop.Plugin.Widgets.UserManuals.Models;
using Nop.Plugin.Widgets.UserManuals.Services;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using System.Linq;

namespace Nop.Plugin.Widgets.UserManuals.Components
{
    [ViewComponent(Name = "WidgetsProductUserManuals")]
    public class WidgetsProductUserManualsViewComponent : NopViewComponent
    {
        private readonly IUserManualService _userManualService;

        public WidgetsProductUserManualsViewComponent(IUserManualService userManualService)
        {
            _userManualService = userManualService;
        }

        public IViewComponentResult Invoke(RouteValueDictionary values)
        {
            var product = (ProductDetailsModel)values["additionalData"];
            var manuals = _userManualService.GetByProductId(product.Id);
            if (manuals != null && manuals.Count() > 0)
            {
                return View("~/Plugins/Widgets.UserManuals/Views/Shared/Components/WidgetProductUserManuals/Default.cshtml",
                    (from manual in manuals
                     select manual.ToModel()
                    ).ToList());
            }
            return Content("");
        }
    }
}
