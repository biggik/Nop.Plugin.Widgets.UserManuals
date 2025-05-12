﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.UserManuals.Models;

public partial record AddProductToUserManualSearchModel : BaseSearchModel
{
    public AddProductToUserManualSearchModel()
    {
    }

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

    public IList<SelectListItem> AvailableCategories { get; set; } = [];

    public IList<SelectListItem> AvailableManufacturers { get; set; } = [];

    public IList<SelectListItem> AvailableStores { get; set; } = [];

    public IList<SelectListItem> AvailableVendors { get; set; } = [];

    public IList<SelectListItem> AvailableProductTypes { get; set; } = [];
}
