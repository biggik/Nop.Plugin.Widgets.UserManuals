using Nop.Plugin.Widgets.UserManuals.Models;
using Nop.Plugin.Widgets.UserManuals.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Core;
using Nop.Services.Messages;
using System.Linq;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Controllers;
using System.Threading.Tasks;
using Nop.Plugin.Widgets.Employees.Constants;

namespace Nop.Plugin.Widgets.UserManuals.Controllers
{
    public partial class CategoriesController : BasePluginController
    {
        public static string ControllerName = nameof(CategoriesController).Replace("Controller", "");
        const string Route = "~/Plugins/Widgets.UserManuals/Views/Categories/";

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
        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            var model = new CategoryModel();
            return View($"{Route}{nameof(Create)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(Areas.Admin)]
        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(CategoryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            if (ModelState.IsValid)
            {
                var category = model.ToEntity();
                await _userManualService.InsertCategoryAsync(category);
                return continueEditing
                    ? RedirectToAction(nameof(Edit), new { id = category.Id })
                    : RedirectToAction(nameof(List));
            }

            //If we got this far, something failed, redisplay form
            return View($"{Route}{nameof(Create)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(Areas.Admin)]
        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

            var category = await _userManualService.GetCategoryByIdAsync(id);
            var model = category.ToModel();
            return View($"{Route}{nameof(Edit)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(Areas.Admin)]
        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(CategoryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(UserManualPermissionProvider.ManageUserManuals))
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
                    ? RedirectToAction(nameof(Edit), new { id = category.Id })
                    : RedirectToAction(nameof(List));
            }

            //If we got this far, something failed, redisplay form
            return View($"{Route}{nameof(Create)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(Areas.Admin)]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            var category = await _userManualService.GetCategoryByIdAsync(id);
            if (category != null)
                await _userManualService.DeleteCategoryAsync(category);

            return RedirectToAction(nameof(List), new { area = "Admin" });
        }

        [AuthorizeAdmin]
        [Area(Areas.Admin)]
        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            var model = new CategorySearchModel();
            model.SetGridPageSize();
            return View($"{Route}{nameof(List)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(Areas.Admin)]
        public async Task<IActionResult> ListData(CategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            var categories = await _userManualService.GetOrderedCategoriesAsync(showUnpublished: true, searchModel.Page - 1, searchModel.PageSize);
            var model = new CategoryListModel().PrepareToGrid(searchModel, categories, () =>
            {
                return categories.Select(category =>
                {
                    var d = category.ToModel();
                    return d;
                });
            });

            return Json(model);
        }
    }
}
