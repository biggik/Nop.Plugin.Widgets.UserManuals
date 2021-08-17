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
        [Area(AreaNames.Admin)]
#if NOP_4_4
        public async Task<IActionResult> Create()
#else
        public IActionResult Create()
#endif
        {
            if (!
#if NOP_4_4
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            var model = new CategoryModel();
            return View($"{Route}{nameof(Create)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
#if NOP_4_4
        public async Task<IActionResult> Create(CategoryModel model, bool continueEditing)
#else
        public IActionResult Create(CategoryModel model, bool continueEditing)
#endif
        {
            if (!
#if NOP_4_4
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
                var category = model.ToEntity();
#if NOP_4_4
                await _userManualService.InsertCategoryAsync(category);
#else
                _userManualService.InsertCategory(category);
#endif
                return continueEditing
                    ? RedirectToAction(nameof(Edit), new { id = category.Id })
                    : RedirectToAction(nameof(List));
            }

            //If we got this far, something failed, redisplay form
            return View($"{Route}{nameof(Create)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
#if NOP_4_4
        public async Task<IActionResult> Edit(int id)
#else
        public IActionResult Edit(int id)
#endif
        {
            if (!
#if NOP_4_4
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
                return AccessDeniedView();

#if NOP_4_4
            var category = await _userManualService.GetCategoryByIdAsync(id);
#else
            var category = _userManualService.GetCategoryById(id);
#endif
            var model = category.ToModel();
            return View($"{Route}{nameof(Edit)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
#if NOP_4_4
        public async Task<IActionResult> Edit(CategoryModel model, bool continueEditing)
#else
        public IActionResult Edit(CategoryModel model, bool continueEditing)
#endif
        {
            if (!
#if NOP_4_4
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
#if NOP_4_4
                var category = await _userManualService.GetCategoryByIdAsync(model.Id);
#else
                var category = _userManualService.GetCategoryById(model.Id);
#endif
                if (category != null)
                {
                    category = model.ToEntity(category);
                }

#if NOP_4_4
                await _userManualService.UpdateCategoryAsync(category);
#else
                _userManualService.UpdateCategory(category);
#endif
                return continueEditing
                    ? RedirectToAction(nameof(Edit), new { id = category.Id })
                    : RedirectToAction(nameof(List));
            }

            //If we got this far, something failed, redisplay form
            return View($"{Route}{nameof(Create)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
#if NOP_4_4
        public async Task<IActionResult> Delete(int id)
#else
        public IActionResult Delete(int id)
#endif
        {
            if (!
#if NOP_4_4
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

#if NOP_4_4
            var category = await _userManualService.GetCategoryByIdAsync(id);
            if (category != null)
                await _userManualService.DeleteCategoryAsync(category);
#else
            var category = _userManualService.GetCategoryById(id);
            if (category != null)
                _userManualService.DeleteCategory(category);
#endif

            return RedirectToAction(nameof(List), new { area = "Admin" });
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
#if NOP_4_4
        public async Task<IActionResult> List()
#else
        public IActionResult List()
#endif
        {
            if (!
#if NOP_4_4
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

            var model = new CategorySearchModel();
            model.SetGridPageSize();
            return View($"{Route}{nameof(List)}.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
#if NOP_4_4
        public async Task<IActionResult> ListData(CategorySearchModel searchModel)
#else
        public IActionResult ListData(CategorySearchModel searchModel)
#endif
        {
            if (!
#if NOP_4_4
                await _permissionService.AuthorizeAsync
#else
                _permissionService.Authorize
#endif
                (UserManualPermissionProvider.ManageUserManuals))
            {
                return AccessDeniedView();
            }

#if NOP_4_4
            var categories = await _userManualService.GetOrderedCategoriesAsync
#else
            var categories = _userManualService.GetOrderedCategories
#endif
                (showUnpublished: true, searchModel.Page - 1, searchModel.PageSize);
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
