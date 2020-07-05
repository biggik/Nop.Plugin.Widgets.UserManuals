using Nop.Plugin.Widgets.UserManuals.Models;
using Nop.Plugin.Widgets.UserManuals.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Controllers;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Core;
using Nop.Services.Media;
using Nop.Services.Messages;
using System.Collections.Generic;
using System;
using Nop.Services.Catalog;
using System.Linq;
using Nop.Services.Seo;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Widgets.UserManuals.Controllers
{
    public partial class UserManualsController : BasePluginController
    {
        public static string ControllerName = nameof(UserManualsController).Replace("Controller", "");
        const string Route = "~/Plugins/Widgets.UserManuals/Views/UserManuals/";

        private readonly IUserManualService _userManualService;
        private readonly IUserManualModelFactory _userManualModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IDownloadService _downloadService;
        private readonly IWorkContext _workContext;

        public UserManualsController(
            IUserManualService userManualService,
            IUserManualModelFactory userManualModelFactory,
            IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IUrlRecordService urlRecordService,
            IDownloadService downloadService,
            IWorkContext workContext)
        {
            _userManualService = userManualService;
            _userManualModelFactory = userManualModelFactory;
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _manufacturerService = manufacturerService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _downloadService = downloadService;
            _workContext = workContext;
        }

        public IActionResult Index()
        {
            var model = new List<ManufacturerManualsModel>();

            var manufacturerDict = _manufacturerService.GetAllManufacturers().ToDictionary(x => x.Id, x => x);
            var categoryDict = _userManualService.GetOrderedCategories(showUnpublished: false).ToDictionary(x => x.Id, x => x);

            string lastManufacturer = "";
            string lastCategoryName = "";
            ManufacturerManualsModel manufacturerModel = null;
            CategoryUserManualModel categoryModel = null;

            foreach (var um in _userManualService.GetOrderedUserManuals(showUnpublished: false))
            {
                var manufacturerName = manufacturerDict.ContainsKey(um.ManufacturerId) ? manufacturerDict[um.ManufacturerId].Name : "";
                if (manufacturerName != lastManufacturer)
                {
                    manufacturerModel = new ManufacturerManualsModel(manufacturerName);
                    model.Add(manufacturerModel);
                    categoryModel = null;
                    lastManufacturer = manufacturerName;
                }

                var categoryName = categoryDict.ContainsKey(um.CategoryId) ? categoryDict[um.CategoryId].Name : "";
                if (categoryName != lastCategoryName || categoryModel == null)
                {
                    categoryModel = new CategoryUserManualModel(new CategoryModel { Name = categoryName });
                    manufacturerModel.Categories.Add(categoryModel);
                    lastCategoryName = categoryName;
                }

                var manualProducts = _userManualService.GetProductsForManual(um.Id);
                if (manualProducts.Any())
                {
                    // We repeat the manual for each product
                    foreach (var umProd in manualProducts)
                    {
                        var umm = um.ToModel();
                        umm.ProductSlug = _urlRecordService.GetActiveSlug(umProd.userManualProduct.ProductId, nameof(Product), 0); // TODO: use languageId ?

                        if (string.IsNullOrEmpty(umm.ProductSlug))
                        {
                            categoryModel.UserManualsForDiscontinuedProducts.Add(umm);
                        }
                        else
                        {
                            umm.ProductDescription = umProd.product.Name;
                            categoryModel.UserManualsForActiveProducts.Add(umm);
                        }
                    }
                }
                else
                {
                    // No products = discontinued product
                    categoryModel.UserManualsForDiscontinuedProducts.Add(um.ToModel());
                }
            }

            if (_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
            {
                DisplayEditLink(Url.Action(nameof(List), ControllerName, new { area = "Admin" }));
            }

            return View($"{Route}{nameof(Index)}.cshtml", model);
        }

        public IActionResult UserManualDownload(int id)
        {
            var download = _downloadService.GetDownloadById(id);
            return File(download.DownloadBinary, download.ContentType, download.Filename + download.Extension);
        }
    }
}
