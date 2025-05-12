using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Widgets.Employees.Constants;
using Nop.Plugin.Widgets.UserManuals.Extensions;
using Nop.Plugin.Widgets.UserManuals.Models;
using Nop.Plugin.Widgets.UserManuals.Resources;
using Nop.Plugin.Widgets.UserManuals.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.UserManuals.Controllers;

public partial class UserManualsController
{
    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    public async Task<IActionResult> ConfigureAsync()
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }
        int storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<UserManualsWidgetSettings>(storeScope);

        var widgetZonesData = GetWidgetZoneData();
        var lookup = widgetZonesData.ToDictionary(x => x.value, y => y.id);

        async Task<List<int>> ZonesAsync(string zones)
        {
            return await (from i in (zones ?? "").Split(';')
                          where lookup.ContainsKey(i)
                          select lookup[i]).ToListAsync();
        }

        async Task<IList<SelectListItem>> AvailableZonesAsync(List<int> zones)
        {
            return await (from wzd in widgetZonesData
                          select new SelectListItem
                          {
                              Text = wzd.name,
                              Value = wzd.id.ToString(),
                              Selected = zones.Contains(wzd.id)
                          }
                   ).ToListAsync();
        }

        var currentWidgetZones = await ZonesAsync(settings.WidgetZones);
        ConfigurationModel model = new()
        {
            WidgetZones = currentWidgetZones,
            AvailableWidgetZones = await AvailableZonesAsync(currentWidgetZones)
        };

        return View($"{Route}{nameof(ConfigureAsync).NoAsync()}.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    [HttpPost, ActionName("Configure")]
    [FormValueRequired("save")]
    public async Task<IActionResult> ConfigureAsync(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }

        if (!ModelState.IsValid)
        {
            return await ConfigureAsync();
        }
        int storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<UserManualsWidgetSettings>(storeScope);

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

        await _settingService.SaveSettingAsync(settings);
        await _settingService.ClearCacheAsync();
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
        return await ConfigureAsync();
    }

    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    public async Task<IActionResult> ListAsync()
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }

        UserManualSearchModel model = new();
        model.SetGridPageSize();

        return View($"{Route}{nameof(ListAsync).NoAsync()}.cshtml", model);
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

    private async Task<IList<SelectListItem>> GetAllAvailableCategoriesAsync(int? selectedCategoryId = null)
    {
        var categories = await _userManualService.GetOrderedCategoriesAsync(showUnpublished: true);

        var list = await (from c in categories
                .Where(cat => cat.Id == (selectedCategoryId ?? -1) || cat.Published)
                          select new SelectListItem
                          {
                              Text = c.Name,
                              Value = c.Id.ToString(),
                              Selected = c.Id == (selectedCategoryId ?? -1)
                          })
                .ToListAsync();
        list.Insert(0, new SelectListItem
        {
            Text = (await _localizationService.GetLocaleStringResourceByNameAsync(GenericResources.None, (await _workContext.GetWorkingLanguageAsync()).Id)).ResourceValue,
            Value = "0"
        });
        return list;
    }

    private async Task<IList<SelectListItem>> GetAllAvailableManufacturersAsync(int selectedManufacturerId)
    {
        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: false);
        var list = await (from m in manufacturers
                .Where(man => man.Id == selectedManufacturerId || man.Published)
                          select new SelectListItem
                          {
                              Text = m.Name,
                              Value = m.Id.ToString(),
                              Selected = m.Id == selectedManufacturerId
                          })
                .ToListAsync();
        list.Insert(0, new SelectListItem
        {
            Text = (await _localizationService.GetLocaleStringResourceByNameAsync(GenericResources.None, (await _workContext.GetWorkingLanguageAsync()).Id)).ResourceValue,
            Value = "0"
        });
        return list;
    }

    private async Task<UserManualModel> PopulateModelListsAsync(UserManualModel model = null)
    {
        if (model == null)
        {
            model = new UserManualModel();
        }

        model.AvailableCategories = await GetAllAvailableCategoriesAsync(model.CategoryId);
        model.AvailableManufacturers = await GetAllAvailableManufacturersAsync(model.ManufacturerId);

        model.UserManualSearchModel.SetGridPageSize();

        model.UserManualProductSearchModel.UserManualId = model.Id;
        model.UserManualProductSearchModel.SetGridPageSize();

        return model;
    }

    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    [HttpPost]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }

        Domain.UserManual userManual = await _userManualService.GetByIdAsync(id);
        if (userManual != null)
        {
            await _userManualService.DeleteUserManualAsync(userManual);
        }

        return RedirectToAction(nameof(IndexAsync).NoAsync(), new { area = "" });
    }

    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    public async Task<IActionResult> CreateAsync()
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }
        UserManualModel model = await PopulateModelListsAsync();
        return View($"{Route}{nameof(CreateAsync).NoAsync()}.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> CreateAsync
        (UserManualModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }

        if (ModelState.IsValid)
        {
            Domain.UserManual userManual = model.ToEntity();

            await _userManualService.InsertUserManualAsync(userManual);
            return continueEditing
                ? RedirectToAction(nameof(EditAsync).NoAsync(), new { id = userManual.Id })
                : RedirectToAction(nameof(ListAsync).NoAsync());
        }

        //If we got this far, something failed, redisplay form
        await PopulateModelListsAsync(model);
        return View($"{Route}{nameof(CreateAsync).NoAsync()}.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    public async Task<IActionResult> EditAsync(int id)
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }
        Domain.UserManual userManual = await _userManualService.GetByIdAsync(id);
        if (userManual == null)
        {
            return RedirectToAction(nameof(IndexAsync).NoAsync(), new { area = "" });
        }

        UserManualModel model = userManual.ToModel();
        await PopulateModelListsAsync(model);
        return View($"{Route}{nameof(EditAsync).NoAsync()}.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> EditAsync
        (UserManualModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }

        if (ModelState.IsValid)
        {
            var userManual = await _userManualService.GetByIdAsync(model.Id);
            if (userManual != null)
            {
                userManual = model.ToEntity(userManual);
            }

            await _userManualService.UpdateUserManualAsync(userManual);
            return continueEditing
                ? RedirectToAction(nameof(EditAsync).NoAsync(), new { id = userManual.Id })
                : RedirectToAction(nameof(ListAsync).NoAsync());
        }

        //If we got this far, something failed, redisplay form
        await PopulateModelListsAsync(model);
        return View($"{Route}{nameof(EditAsync).NoAsync()}.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    public async Task<IActionResult> ListDataAsync(CategorySearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }

        var manufacturerDict = (await _manufacturerService.GetAllManufacturersAsync(showHidden: true)).ToDictionary(x => x.Id, x => x);
        var categoryDict = (await _userManualService.GetOrderedCategoriesAsync(showUnpublished: true)).ToDictionary(x => x.Id, x => x);

        var userManuals = await _userManualService.GetOrderedUserManualsAsync(showUnpublished: true, searchModel.Page - 1, searchModel.PageSize);
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
    [Area(Areas.Admin)]
    [HttpPost]
    public async Task<IActionResult> ListProductDataAsync(UserManualProductSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }

        var data = await _userManualService.GetProductsForManualAsync
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
    [Area(Areas.Admin)]
    public virtual async Task<IActionResult> ProductDeleteAsync(int userManualId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }

        //try to get a discount with the specified id
        var manual = await _userManualService.GetByIdAsync(userManualId)
            ?? throw new ArgumentException("No user manual with the specified id found", nameof(userManualId));

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId)
            ?? throw new ArgumentException("No product found with the specified id", nameof(productId));

        //remove discount
        await _userManualService.RemoveProductFromManualAsync(userManualId, productId);

        return new NullJsonResult();
    }

    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    public virtual async Task<IActionResult> ProductAddPopupAsync(int userManualId)
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }

        //prepare model
        var model = await _userManualModelFactory.PrepareAddProductToUserManualSearchModelAsync(new AddProductToUserManualSearchModel());

        return View($"{Route}{nameof(ProductAddPopupAsync).NoAsync()}.cshtml", model);
    }

    [HttpPost]
    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    public virtual async Task<IActionResult> ProductAddPopupListAsync(AddProductToUserManualSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return await base.AccessDeniedJsonAsync();
        }
        //prepare model
        var model = await _userManualModelFactory.PrepareAddProductToUserManualListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> ProductAddPopupAsync(AddProductToUserManualModel model)
    {
        if (!await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            return AccessDeniedView();
        }

        //try to get a discount with the specified id
        var discount = await _userManualService.GetByIdAsync(model.UserManualId)
            ?? throw new ArgumentException("No user manual found with the specified id");

        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            foreach (Core.Domain.Catalog.Product product in selectedProducts)
            {
                await _userManualService.AddProductToManualAsync(model.UserManualId, product.Id);
            }
        }

        TempData["RefreshPage"] = true;
        TempData["BtnId"] = Request.Query["btnId"].First();
        return RedirectToAction(nameof(ProductAddPopupAsync).NoAsync(), new { id = model.UserManualId, area = "admin" });
    }
}
