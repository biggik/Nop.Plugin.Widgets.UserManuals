using Nop.Plugin.Widgets.UserManuals.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Controllers;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Core;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            var model = await _userManualService.GetOrderedUserManualsWithProductsAsync(showUnpublished: false);
            if (await _permissionService.AuthorizeAsync(UserManualPermissionProvider.ManageUserManuals))
            {
                DisplayEditLink(Url.Action(nameof(List), ControllerName, new { area = "Admin" }));
            }

            return View($"{Route}{nameof(Index)}.cshtml", model);
        }

        public async Task<IActionResult> UserManualDownload(int id)
        {
            var download = await _downloadService.GetDownloadByIdAsync(id);
            return File(download.DownloadBinary, download.ContentType, download.Filename + download.Extension);
        }
    }
}
