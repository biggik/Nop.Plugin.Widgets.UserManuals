using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Plugin.Widgets.UserManuals.Resources;
using System.ComponentModel.DataAnnotations;
using System;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
    public class UserManualModel : BaseNopEntityModel
    {
        public UserManualModel()
        {
        }

        [NopResourceDisplayName(UserManualResources.Manufacturer)]
        public int ManufacturerId { get; set; }

        [NopResourceDisplayName(UserManualResources.Category)]
        public int CategoryId { get; set; }

        [NopResourceDisplayName(UserManualResources.Description)]
        public string Description { get; set; }

        [NopResourceDisplayName(UserManualResources.Document)]
        [UIHint("Download")]
        public int DocumentId { get; set; }

        [NopResourceDisplayName(UserManualResources.OnlineLink)]
        public string OnlineLink { get; set; }

        [NopResourceDisplayName(GenericResources.Published)]
        public bool Published { get; set; } = true;

        [NopResourceDisplayName(GenericResources.DisplayOrder)]
        public int DisplayOrder { get; set; }

        public bool ManufacturerPublished { get; set; }
        public string ManufacturerName { get; set; }

        public bool CategoryPublished { get; set; }
        public string CategoryName { get; set; }

        public string ProductDescription { get; set; }
        public string ProductSlug { get; set; }

        public string SortDescription => ProductDescription ?? Description;
        public bool IsDiscontinuedProduct => string.IsNullOrEmpty(ProductDescription);

        public IList<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem> AvailableManufacturers { get; set; } = new List<SelectListItem>();

        public UserManualSearchModel UserManualSearchModel { get; } = new UserManualSearchModel();
        public UserManualProductSearchModel UserManualProductSearchModel { get; } = new UserManualProductSearchModel();
    }
}