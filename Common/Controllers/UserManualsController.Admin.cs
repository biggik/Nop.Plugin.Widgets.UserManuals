using System.Linq;
using System.Reflection;
using Nop.Plugin.Widgets.UserManuals.Models;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Widgets.UserManuals.Services;
using Nop.Web.Framework.Models.Extensions;
using Nop.Plugin.Widgets.UserManuals.Resources;
using System;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Widgets.UserManuals.Controllers
{
    public partial class UserManualsController
    {
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return BadRequest();

            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var settings = _settingService.LoadSetting<UserManualsWidgetSettings>(storeScope);

            var widgetZonesData = GetWidgetZoneData();
            var lookup = widgetZonesData.ToDictionary(x => x.value, y => y.id);

            List<int> Zones(string zones)
            {
                return (from i in (zones ?? "").Split(';')
                        where lookup.ContainsKey(i)
                        select lookup[i]).ToList();
            }

            IList<SelectListItem> AvailableZones(List<int> zones)
            {
                return (from wzd in widgetZonesData
                        select new SelectListItem
                        {
                            Text = wzd.name,
                            Value = wzd.id.ToString(),
                            Selected = zones.Contains(wzd.id)
                        }
                       ).ToList();
            }

            var currentWidgetZones = Zones(settings.WidgetZones);
            var model = new ConfigurationModel
            {
                WidgetZones = currentWidgetZones,
                AvailableWidgetZones = AvailableZones(currentWidgetZones)
            };

            return View($"{Route}{nameof(Configure)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return BadRequest();

            if (!ModelState.IsValid)
                return Configure();

            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var settings = _settingService.LoadSetting<UserManualsWidgetSettings>(storeScope);

            var widgetZonesData = GetWidgetZoneData();
            var lookup = widgetZonesData.ToDictionary(x => x.id, y => y.value);

            string Join(IList<int> zones)
            {
                return model.WidgetZones != null && model.WidgetZones.Any()
                        ? string.Join(";",
                                from i in zones
                                where lookup.ContainsKey(i)
                                select lookup[i]
                                )
                        : "";
            }

            settings.WidgetZones = Join(model.WidgetZones);

            _settingService.SaveSetting(settings);
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult List()
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

            var model = new UserManualSearchModel();
            model.SetGridPageSize();

            return View($"{Route}{nameof(List)}.cshtml", model);
        }

        private List<(string name, string value, int id)> GetWidgetZoneData()
        {
            int id = 1000;
            return typeof(Nop.Web.Framework.Infrastructure.PublicWidgetZones)
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .OrderBy(x => x.Name)
                .Select(x => (name: x.Name, value: x.GetValue(null, null).ToString(), id++))
                .ToList();
        }
        
        private IList<SelectListItem> GetAllAvailableCategories(int? selectedCategoryId = null)
        {
            var list = (from c in _userManualService.GetOrderedCategories(showUnpublished: true)
                    .Where(cat => cat.Id == (selectedCategoryId ?? -1) || cat.Published)
                    select new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == (selectedCategoryId ?? -1)
                    })
                    .ToList();
            list.Insert(0, new SelectListItem
            {
                Text = _localizationService.GetLocaleStringResourceByName(GenericResources.None).ResourceValue,
                Value = "0"
            });
            return list;
        }

        private IList<SelectListItem> GetAllAvailableManufacturers(int selectedManufacturerId)
        {
            var list = (from m in _manufacturerService.GetAllManufacturers(showHidden: false)
                    .Where(man => man.Id == selectedManufacturerId || man.Published)
                     select new SelectListItem
                     {
                         Text = m.Name,
                         Value = m.Id.ToString(),
                         Selected = m.Id == selectedManufacturerId
                     })
                    .ToList();
            list.Insert(0, new SelectListItem
            {
                Text = _localizationService.GetLocaleStringResourceByName(GenericResources.None).ResourceValue,
                Value = "0"
            });
            return list;
        }

        private UserManualModel PopulateModelLists(UserManualModel model = null)
        {
            if (model == null) model = new UserManualModel();
            
            model.AvailableCategories = GetAllAvailableCategories(model.CategoryId);
            model.AvailableManufacturers = GetAllAvailableManufacturers(model.ManufacturerId);
            
            model.UserManualSearchModel.SetGridPageSize();

            model.UserManualProductSearchModel.UserManualId = model.Id;
            model.UserManualProductSearchModel.SetGridPageSize();
            
            return model;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

            var userManual = _userManualService.GetById(id);
            if (userManual != null)
                _userManualService.DeleteUserManual(userManual);

            return RedirectToAction(nameof(Index), new { area = "" });
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

            var model = PopulateModelLists();
            return View($"{Route}{nameof(Create)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(UserManualModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var userManual = model.ToEntity();

                _userManualService.InsertUserManual(userManual);
                return continueEditing
                    ? RedirectToAction(nameof(Edit), new { id = userManual.Id })
                    : RedirectToAction(nameof(List));
            }

            //If we got this far, something failed, redisplay form
            PopulateModelLists(model);
            return View($"{Route}{nameof(Create)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

            var userManual = _userManualService.GetById(id);
            if (userManual == null)
            {
                return RedirectToAction(nameof(Index), new { area = "" });
            }

            var model = userManual.ToModel();
            PopulateModelLists(model);
            return View($"{Route}{nameof(Edit)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(UserManualModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var userManual = _userManualService.GetById(model.Id);
                if (userManual != null)
                {
                    userManual = model.ToEntity(userManual);
                }

                _userManualService.UpdateUserManual(userManual);
                return continueEditing
                    ? RedirectToAction(nameof(Edit), new { id = userManual.Id })
                    : RedirectToAction(nameof(List));
            }

            //If we got this far, something failed, redisplay form
            PopulateModelLists(model);
            return View($"{Route}{nameof(Edit)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult ListData(CategorySearchModel searchModel)
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

            var manufacturerDict = _manufacturerService.GetAllManufacturers(showHidden: true).ToDictionary(x => x.Id, x => x);
            var categoryDict = _userManualService.GetOrderedCategories(showUnpublished: true).ToDictionary(x => x.Id, x => x);

            var userManuals = _userManualService.GetOrderedUserManuals(showUnpublished: true, searchModel.Page - 1, searchModel.PageSize);
            var model = new UserManualListModel().PrepareToGrid(searchModel, userManuals, () =>
            {
                return userManuals.Select(userManual =>
                {
                    var um = userManual.ToModel();

                    um.CategoryName = categoryDict.ContainsKey(um.CategoryId) ? categoryDict[um.CategoryId].Name : "";
                    um.CategoryPublished = categoryDict.ContainsKey(um.CategoryId) && categoryDict[um.CategoryId].Published;

                    um.ManufacturerName = manufacturerDict.ContainsKey(um.ManufacturerId) ? manufacturerDict[um.ManufacturerId].Name : "";
                    um.ManufacturerPublished = manufacturerDict.ContainsKey(um.ManufacturerId) && manufacturerDict[um.ManufacturerId].Published;

                    return um;
                });
            });

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        public IActionResult ListProductData(UserManualProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

            var data = _userManualService.GetProductsForManual(searchModel.UserManualId, showUnpublished: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new UserManualProductListModel().PrepareToGrid(searchModel, data, () =>
            {
                //fill in model values from the entity
                return data.Select(pm =>
                {
                    var pmm = pm.userManualProduct.ToModel();
                    pmm.ProductName = pm.product.Name;
                    pmm.Published = pm.product.Published;
                    return pmm;
                });
            });

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult ProductDelete(int userManualId, int productId)
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

            //try to get a discount with the specified id
            var manual = _userManualService.GetById(userManualId)
                ?? throw new ArgumentException("No user manual with the specified id found", nameof(userManualId));

            //try to get a product with the specified id
            var product = _productService.GetProductById(productId)
                ?? throw new ArgumentException("No product found with the specified id", nameof(productId));

            //remove discount
            _userManualService.RemoveProductFromManual(userManualId, productId);

            return new NullJsonResult();
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult ProductAddPopup(int userManualId)
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

            //prepare model
            var model = _userManualModelFactory.PrepareAddProductToUserManualSearchModel(new AddProductToUserManualSearchModel());

            return View($"{Route}{nameof(ProductAddPopup)}.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult ProductAddPopupList(AddProductToUserManualSearchModel searchModel)
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _userManualModelFactory.PrepareAddProductToUserManualListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [FormValueRequired("save")]
        public virtual IActionResult ProductAddPopup(AddProductToUserManualModel model)
        {
            if (!_permissionService.Authorize(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

            //try to get a discount with the specified id
            var discount = _userManualService.GetById(model.UserManualId)
                ?? throw new ArgumentException("No user manual found with the specified id");

            var selectedProducts = _productService.GetProductsByIds(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                foreach (var product in selectedProducts)
                {
                    _userManualService.AddProductToManual(model.UserManualId, product.Id);
                }
            }

            TempData["RefreshPage"] = true;
            TempData["BtnId"] = Request.Query["btnId"].First();
            return RedirectToAction(nameof(ProductAddPopup), new { id = model.UserManualId, area = "admin" });
        }

    }
}
