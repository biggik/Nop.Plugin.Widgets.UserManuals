using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Widgets.UserManuals.Resources;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
    public partial record UserManualSearchModel : BaseSearchModel
    {
        #region Ctor

        public UserManualSearchModel()
        {
        }

        #endregion

        #region Properties

        [NopResourceDisplayName(UserManualResources.SearchName)]
        public string SearchManualName { get; set; }

        [NopResourceDisplayName(UserManualResources.Manufacturer)]
        public int SearchManufacturerId { get; set; }

        [NopResourceDisplayName(UserManualResources.Category)]
        public int SearchCategoryId { get; set; }

        public IList<SelectListItem> AvailableManufacturers { get; set; } = [];

        public IList<SelectListItem> AvailableCategories { get; set; } = [];
        #endregion
    }
}
