using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.Employees.Constants;
using Nop.Plugin.Widgets.UserManuals.Extensions;
using Nop.Plugin.Widgets.UserManuals.Models;
using Nop.Plugin.Widgets.UserManuals.Security;
using Nop.Plugin.Widgets.UserManuals.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.UserManuals.Controllers;

public partial class CategoriesController : BasePluginController
{
    public static string ControllerName = nameof(CategoriesController).NoController();
    private const string Route = "~/Plugins/Widgets.UserManuals/Views/Categories/";

    private readonly IUserManualService _userManualService;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IWorkContext _workContext;

    public CategoriesController(
        IUserManualService userManualService,
        IStoreContext storeContext,
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IWorkContext workContext)
    {
        _userManualService = userManualService;
        _storeContext = storeContext;
        _settingService = settingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _workContext = workContext;
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

        return View($"{Route}{nameof(CreateAsync).NoAsync()}.cshtml", new CategoryModel());
    }

    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> CreateAsync(CategoryModel model, bool continueEditing)
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
            Domain.UserManualCategory category = model.ToEntity();
            await _userManualService.InsertCategoryAsync(category);
            return continueEditing
                ? RedirectToAction(nameof(EditAsync).NoAsync(), new { id = category.Id })
                : RedirectToAction(nameof(ListAsync).NoAsync());
        }

        //If we got this far, something failed, redisplay form
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
            return AccessDeniedView();

        Domain.UserManualCategory category = await _userManualService.GetCategoryByIdAsync(id);
        CategoryModel model = category.ToModel();
        return View($"{Route}{nameof(EditAsync).NoAsync()}.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(Areas.Admin)]
    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> EditAsync(CategoryModel model, bool continueEditing)
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
            var category = await _userManualService.GetCategoryByIdAsync(model.Id);
            if (category != null)
            {
                category = model.ToEntity(category);
            }

            await _userManualService.UpdateCategoryAsync(category);
            return continueEditing
                ? RedirectToAction(nameof(EditAsync).NoAsync(), new { id = category.Id })
                : RedirectToAction(nameof(ListAsync).NoAsync());
        }

        //If we got this far, something failed, redisplay form
        return View($"{Route}{nameof(CreateAsync).NoAsync()} .cshtml", model);
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

        Domain.UserManualCategory category = await _userManualService.GetCategoryByIdAsync(id);
        if (category != null)
        {
            await _userManualService.DeleteCategoryAsync(category);
        }
        return RedirectToAction(nameof(ListAsync).NoAsync(), new { area = "Admin" });
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

        CategorySearchModel model = new();
        model.SetGridPageSize();
        return View($"{Route}{nameof(ListAsync).NoAsync()}.cshtml", model);
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

        var categories = await _userManualService.GetOrderedCategoriesAsync(showUnpublished: true, searchModel.Page - 1, searchModel.PageSize);
        var model = new CategoryListModel().PrepareToGrid(searchModel, categories, () =>
        {
            return categories.Select(category => category.ToModel());
        });

        return Json(model);
    }
}
