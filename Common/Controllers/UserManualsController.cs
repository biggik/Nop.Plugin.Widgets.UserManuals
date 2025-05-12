using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.UserManuals.Extensions;
using Nop.Plugin.Widgets.UserManuals.Security;
using Nop.Plugin.Widgets.UserManuals.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Widgets.UserManuals.Controllers;

public partial class UserManualsController : BasePluginController
{
    public static string ControllerName = nameof(UserManualsController).NoController();
    private const string Route = "~/Plugins/Widgets.UserManuals/Views/UserManuals/";

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

    public async Task<IActionResult> IndexAsync()
    {
        var model = await _userManualService.GetOrderedUserManualsWithProductsAsync(showUnpublished: false).ConfigureAwait(false);
        if (await _permissionService.AuthorizeAsync(
#if NOP_47
            UserManualPermissionProvider.ManageUserManuals))
#else
            UserManualPermissionConfigs.MANAGE_USER_MANUALS))
#endif
        {
            DisplayEditLink(Url.Action(nameof(ListAsync).NoAsync(), ControllerName, new { area = "Admin" }));
        }

        return View($"{Route}{nameof(IndexAsync).NoAsync()}.cshtml", model);
    }

    public async Task<IActionResult> UserManualDownloadAsync(int id)
    {
        var download = await _downloadService.GetDownloadByIdAsync(id);
        return File(download.DownloadBinary, download.ContentType, download.Filename + download.Extension);
    }
}
