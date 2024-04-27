using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
    public partial record AddProductToUserManualSearchModel : BaseSearchModel
    {
        #region Ctor

        public AddProductToUserManualSearchModel()
        {
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
        public string SearchProductName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
        public int SearchCategoryId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
        public int SearchManufacturerId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
        public int SearchStoreId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
        public int SearchVendorId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
        public int SearchProductTypeId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

        public IList<SelectListItem> AvailableManufacturers { get; set; } = new List<SelectListItem>();

        public IList<SelectListItem> AvailableStores { get; set; } = new List<SelectListItem>();

        public IList<SelectListItem> AvailableVendors { get; set; } = new List<SelectListItem>();

        public IList<SelectListItem> AvailableProductTypes { get; set; } = new List<SelectListItem>();

        #endregion
    }
}
