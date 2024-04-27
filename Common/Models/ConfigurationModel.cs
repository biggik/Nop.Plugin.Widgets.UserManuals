using Nop.Plugin.Widgets.UserManuals.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
    public partial record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
        }

        [NopResourceDisplayName(ConfigurationResources.WidgetZones)]
        public IList<int> WidgetZones { get; set; }

        public IList<SelectListItem> AvailableWidgetZones { get; set; }
    }
}