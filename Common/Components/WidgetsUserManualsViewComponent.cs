using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Nop.Plugin.Widgets.UserManuals.Models;
using Nop.Plugin.Widgets.UserManuals.Services;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using System.Collections;
using System.Linq;

namespace Nop.Plugin.Widgets.UserManuals.Components
{
    [ViewComponent(Name = "WidgetsUserManuals")]
    public class WidgetsUserManualsViewComponent : NopViewComponent
    {
        private readonly IUserManualService _userManualService;

        public WidgetsUserManualsViewComponent(IUserManualService userManualService)
        {
            _userManualService = userManualService;
        }

        public IViewComponentResult Invoke(RouteValueDictionary values)
        {
            var product = (ProductDetailsModel)values["additionalData"];
            var manuals = _userManualService.GetByProductId(product.Id);
            if (manuals != null && manuals.Count() > 0)
            {
                return View("~/Plugins/Widgets.UserManuals/Views/Shared/Components/WidgetUserManuals/Default.cshtml",
                    (from manual in manuals
                     select manual.ToModel()
                    ).ToList());
            }
            return Content("");
        }
    }
}
