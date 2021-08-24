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
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.UserManuals.Controllers
{
    public partial class UserManualsController
    {
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
#if NOP_ASYNC
        public async Task<IActionResult> Configure()
#else
        public IActionResult Configure()
#endif
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }
#if NOP_ASYNC
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<UserManualsWidgetSettings>(storeScope);
#else
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var settings = _settingService.LoadSetting<UserManualsWidgetSettings>(storeScope);
#endif

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
#if NOP_ASYNC
        public async Task<IActionResult> Configure(ConfigurationModel model)
#else
        public IActionResult Configure(ConfigurationModel model)
#endif
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            if (!ModelState.IsValid)
            {
#if NOP_ASYNC
                return await Configure();
#else
                return Configure();
#endif
            }
#if NOP_ASYNC
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<UserManualsWidgetSettings>(storeScope);
#else
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var settings = _settingService.LoadSetting<UserManualsWidgetSettings>(storeScope);
#endif

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

#if NOP_ASYNC
            await _settingService.SaveSettingAsync(settings);
            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            return await Configure();
#else
            _settingService.SaveSetting(settings);
            _settingService.ClearCache();
            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
#endif

        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
#if NOP_ASYNC
        public async Task<IActionResult> List()
#else
        public IActionResult List()
#endif
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

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

#if NOP_ASYNC
        private async Task<IList<SelectListItem>> GetAllAvailableCategoriesAsync(int? selectedCategoryId = null)
#else
        private IList<SelectListItem> GetAllAvailableCategories(int? selectedCategoryId = null)
#endif
        {
            Core.IPagedList<Domain.UserManualCategory> categories =
#if NOP_ASYNC
                await _userManualService.GetOrderedCategoriesAsync(showUnpublished: true);
#else
                _userManualService.GetOrderedCategories(showUnpublished: true);
#endif

            var list = (from c in categories
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
#if NOP_ASYNC
                Text = (await _localizationService.GetLocaleStringResourceByNameAsync(GenericResources.None, (await _workContext.GetWorkingLanguageAsync()).Id)).ResourceValue,
#else
                Text = _localizationService.GetLocaleStringResourceByName(GenericResources.None).ResourceValue,
#endif
                Value = "0"
            });
            return list;
        }

#if NOP_ASYNC
        private async Task<IList<SelectListItem>> GetAllAvailableManufacturersAsync(int selectedManufacturerId)
#else
        private IList<SelectListItem> GetAllAvailableManufacturers(int selectedManufacturerId)
#endif
        {
            var manufacturers =
#if NOP_ASYNC
                await _manufacturerService.GetAllManufacturersAsync(showHidden: false);
#else
                _manufacturerService.GetAllManufacturers(showHidden: false);
#endif
            var list = (from m in manufacturers
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
#if NOP_ASYNC
                Text = (await _localizationService.GetLocaleStringResourceByNameAsync(GenericResources.None, (await _workContext.GetWorkingLanguageAsync()).Id)).ResourceValue,
#else
                Text = _localizationService.GetLocaleStringResourceByName(GenericResources.None).ResourceValue,
#endif
                Value = "0"
            });
            return list;
        }

#if NOP_ASYNC
        private async Task<UserManualModel> PopulateModelListsAsync(UserManualModel model = null)
#else
        private UserManualModel PopulateModelLists(UserManualModel model = null)
#endif
        {
            if (model == null) model = new UserManualModel();

#if NOP_ASYNC
            model.AvailableCategories = await GetAllAvailableCategoriesAsync(model.CategoryId);
            model.AvailableManufacturers = await GetAllAvailableManufacturersAsync(model.ManufacturerId);
#else
            model.AvailableCategories = GetAllAvailableCategories(model.CategoryId);
            model.AvailableManufacturers = GetAllAvailableManufacturers(model.ManufacturerId);
#endif

            model.UserManualSearchModel.SetGridPageSize();

            model.UserManualProductSearchModel.UserManualId = model.Id;
            model.UserManualProductSearchModel.SetGridPageSize();
            
            return model;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
#if NOP_ASYNC
        public async Task<IActionResult> Delete(int id)
#else
        public IActionResult Delete(int id)
#endif
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

#if NOP_ASYNC
            var userManual = await _userManualService.GetByIdAsync(id);
            if (userManual != null)
                await _userManualService.DeleteUserManualAsync(userManual);
#else
            var userManual = _userManualService.GetById(id);
            if (userManual != null)
                _userManualService.DeleteUserManual(userManual);
#endif

            return RedirectToAction(nameof(Index), new { area = "" });
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
#if NOP_ASYNC
        public async Task<IActionResult> Create()
#else
        public IActionResult Create()
#endif
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }
#if NOP_ASYNC
            var model = await PopulateModelListsAsync();
#else
            var model = PopulateModelLists();
#endif
            return View($"{Route}{nameof(Create)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
#if NOP_ASYNC
        public async Task<IActionResult> Create
#else
        public IActionResult Create
#endif
            (UserManualModel model, bool continueEditing)
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            if (ModelState.IsValid)
            {
                var userManual = model.ToEntity();

#if NOP_ASYNC
                await _userManualService.InsertUserManualAsync(userManual);
#else
                _userManualService.InsertUserManual(userManual);
#endif
                return continueEditing
                    ? RedirectToAction(nameof(Edit), new { id = userManual.Id })
                    : RedirectToAction(nameof(List));
            }

            //If we got this far, something failed, redisplay form
#if NOP_ASYNC
            await PopulateModelListsAsync(model);
#else
            PopulateModelLists(model);
#endif
            return View($"{Route}{nameof(Create)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
#if NOP_ASYNC
        public async Task<IActionResult> Edit(int id)
#else
        public IActionResult Edit(int id)
#endif
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }
#if NOP_ASYNC
            var userManual = await _userManualService.GetByIdAsync(id);
#else
            var userManual = _userManualService.GetById(id);
#endif
            if (userManual == null)
            {
                return RedirectToAction(nameof(Index), new { area = "" });
            }

            var model = userManual.ToModel();
#if NOP_ASYNC
            await PopulateModelListsAsync(model);
#else
            PopulateModelLists(model);
#endif
            return View($"{Route}{nameof(Edit)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
#if NOP_ASYNC
        public async Task<IActionResult> Edit
#else
        public IActionResult Edit
#endif
            (UserManualModel model, bool continueEditing)
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            if (ModelState.IsValid)
            {
#if NOP_ASYNC
                var userManual = await _userManualService.GetByIdAsync(model.Id);
#else
                var userManual = _userManualService.GetById(model.Id);
#endif
                if (userManual != null)
                {
                    userManual = model.ToEntity(userManual);
                }

#if NOP_ASYNC
                await _userManualService.UpdateUserManualAsync(userManual);
#else
                _userManualService.UpdateUserManual(userManual);
#endif
                return continueEditing
                    ? RedirectToAction(nameof(Edit), new { id = userManual.Id })
                    : RedirectToAction(nameof(List));
            }

            //If we got this far, something failed, redisplay form
#if NOP_ASYNC
            await PopulateModelListsAsync(model);
#else
            PopulateModelLists(model);
#endif
            return View($"{Route}{nameof(Edit)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
#if NOP_ASYNC
        public async Task<IActionResult> ListData(CategorySearchModel searchModel)
#else
        public IActionResult ListData(CategorySearchModel searchModel)
#endif
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

#if NOP_ASYNC
            var manufacturerDict = (await _manufacturerService.GetAllManufacturersAsync(showHidden: true)).ToDictionary(x => x.Id, x => x);
            var categoryDict = (await _userManualService.GetOrderedCategoriesAsync(showUnpublished: true)).ToDictionary(x => x.Id, x => x);

            var userManuals = await _userManualService.GetOrderedUserManualsAsync(showUnpublished: true, searchModel.Page - 1, searchModel.PageSize);
#else
            var manufacturerDict = _manufacturerService.GetAllManufacturers(showHidden: true).ToDictionary(x => x.Id, x => x);
            var categoryDict = _userManualService.GetOrderedCategories(showUnpublished: true).ToDictionary(x => x.Id, x => x);

            var userManuals = _userManualService.GetOrderedUserManuals(showUnpublished: true, searchModel.Page - 1, searchModel.PageSize);
#endif
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
#if NOP_ASYNC
        public async Task<IActionResult> ListProductData(UserManualProductSearchModel searchModel)
#else
        public IActionResult ListProductData(UserManualProductSearchModel searchModel)
#endif
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

#if NOP_ASYNC
            var data = await _userManualService.GetProductsForManualAsync
#else
            var data = _userManualService.GetProductsForManual
#endif
                (searchModel.UserManualId, showUnpublished: true,
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
#if NOP_ASYNC
        public virtual async Task<IActionResult> ProductDelete(int userManualId, int productId)
#else
        public virtual IActionResult ProductDelete(int userManualId, int productId)
#endif
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            //try to get a discount with the specified id
#if NOP_ASYNC
            var manual = await _userManualService.GetByIdAsync(userManualId)
#else
            var manual = _userManualService.GetById(userManualId)
#endif
                ?? throw new ArgumentException("No user manual with the specified id found", nameof(userManualId));

            //try to get a product with the specified id
#if NOP_ASYNC
            var product = await _productService.GetProductByIdAsync(productId)
#else
            var product = _productService.GetProductById(productId)
#endif
                ?? throw new ArgumentException("No product found with the specified id", nameof(productId));

            //remove discount
#if NOP_ASYNC
            await _userManualService.RemoveProductFromManualAsync(userManualId, productId);
#else
            _userManualService.RemoveProductFromManual(userManualId, productId);
#endif

            return new NullJsonResult();
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
#if NOP_ASYNC
        public virtual async Task<IActionResult> ProductAddPopup(int userManualId)
#else
        public virtual IActionResult ProductAddPopup(int userManualId)
#endif
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            //prepare model
#if NOP_ASYNC
            var model = await _userManualModelFactory.PrepareAddProductToUserManualSearchModelAsync
#else
            var model = _userManualModelFactory.PrepareAddProductToUserManualSearchModel
#endif
                (new AddProductToUserManualSearchModel());

            return View($"{Route}{nameof(ProductAddPopup)}.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
#if NOP_ASYNC
        public virtual async Task<IActionResult> ProductAddPopupList(AddProductToUserManualSearchModel searchModel)
#else
        public virtual IActionResult ProductAddPopupList(AddProductToUserManualSearchModel searchModel)
#endif
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
#if NOP_ASYNC
                return await AccessDeniedDataTablesJson();
#else
                return AccessDeniedDataTablesJson();
#endif
            }
            //prepare model
#if NOP_ASYNC
            var model = await _userManualModelFactory.PrepareAddProductToUserManualListModelAsync(searchModel);
#else
            var model = _userManualModelFactory.PrepareAddProductToUserManualListModel(searchModel);
#endif

            return Json(model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [FormValueRequired("save")]
#if NOP_ASYNC
        public virtual async Task<IActionResult> ProductAddPopup(AddProductToUserManualModel model)
#else
        public virtual IActionResult ProductAddPopup(AddProductToUserManualModel model)
#endif
        {
            if (!
#if NOP_ASYNC
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            //try to get a discount with the specified id
#if NOP_ASYNC
            var discount = await _userManualService.GetByIdAsync(model.UserManualId)
#else
            var discount = _userManualService.GetById(model.UserManualId)
#endif
                ?? throw new ArgumentException("No user manual found with the specified id");

#if NOP_ASYNC
            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
#else
            var selectedProducts = _productService.GetProductsByIds(model.SelectedProductIds.ToArray());
#endif
            if (selectedProducts.Any())
            {
                foreach (var product in selectedProducts)
                {
#if NOP_ASYNC
                    await _userManualService.AddProductToManualAsync(model.UserManualId, product.Id);
#else
                    _userManualService.AddProductToManual(model.UserManualId, product.Id);
#endif
                }
            }

            TempData["RefreshPage"] = true;
            TempData["BtnId"] = Request.Query["btnId"].First();
            return RedirectToAction(nameof(ProductAddPopup), new { id = model.UserManualId, area = "admin" });
        }
    }
}
