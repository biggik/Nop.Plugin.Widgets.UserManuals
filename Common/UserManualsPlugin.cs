using System.Linq;
using System.Collections.Generic;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Plugin.Widgets.UserManuals.Data;
using Nop.Plugin.Widgets.UserManuals.Services;
using Nop.Core.Infrastructure;
using Nop.Core.Domain.Localization;
using Nop.Core;
using Nop.Services.Plugins;
using Nop.Plugin.Widgets.UserManuals.Resources;
using Nop.Web.Framework.Menu;
using System;
using Microsoft.AspNetCore.Routing;
using Nop.Services.Security;
using Nop.Plugin.Widgets.UserManuals.Controllers;
using nopLocalizationHelper;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.UserManuals
{
    public class UserManualPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {
#if NOP_PRE_4_3
        private readonly UserManualsObjectContext _objectContext;
#endif
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IPermissionService _permissionService;

        public bool HideInWidgetList => false;

        public UserManualPlugin(
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IPermissionService permissionService,
#if NOP_PRE_4_3
            UserManualsObjectContext objectContext,
#endif
            IStoreContext storeContext,
            ISettingService settingService)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _languageService = languageService;
            _permissionService = permissionService;
#if NOP_PRE_4_3
            _objectContext = objectContext;
#endif
            _settingService = settingService;
#if DEBUG
#if NOP_4_4
            // TODO remove .RunSynchronously below
            CreateLocaleStringsAsync().RunSynchronously();
            _permissionService.InstallPermissionsAsync(new UserManualPermissionProvider()).RunSynchronously();
#else
            CreateLocaleStrings();
            _permissionService.InstallPermissions(new UserManualPermissionProvider());
#endif
#endif

#if NOP_4_4
            // TODO remove .Result below
            var storeScope =  storeContext.GetActiveStoreScopeConfigurationAsync().Result;
            var settings = _settingService.LoadSettingAsync<UserManualsWidgetSettings>(storeScope).Result;
#else
            var storeScope = storeContext.ActiveStoreScopeConfiguration;
            var settings = _settingService.LoadSetting<UserManualsWidgetSettings>(storeScope);
#endif

            _widgetZones = string.IsNullOrWhiteSpace(settings.WidgetZones)
                ? new List<string>()
                : settings.WidgetZones.Split(';').ToList();
        }

        private LocaleStringHelper<LocaleStringResource> ResourceHelper()
        {
            return new LocaleStringHelper<LocaleStringResource>
            (
#if NOP_4_4
                pluginAssembly: GetType().Assembly,
                languageCultures: from lang in _languageService.GetAllLanguagesAsync().Result select (lang.Id, lang.LanguageCulture),
                getResource: (resourceName, languageId) => _localizationService.GetLocaleStringResourceByNameAsync(resourceName, languageId, false),
                createResource: (languageId, resourceName, resourceValue) => new LocaleStringResource { LanguageId = languageId, ResourceName = resourceName, ResourceValue = resourceValue },
                insertResource: (lsr) => _localizationService.InsertLocaleStringResourceAsync(lsr),
                updateResource: (lsr, resourceValue) => { lsr.ResourceValue = resourceValue; return _localizationService.UpdateLocaleStringResourceAsync(lsr); },
                deleteResource: (lsr) => _localizationService.DeleteLocaleStringResourceAsync(lsr),
                areResourcesEqual: (lsr, resourceValue) => lsr.ResourceValue == resourceValue
#else
                pluginAssembly: GetType().Assembly,
                languageCultures: from lang in _languageService.GetAllLanguages() select (lang.Id, lang.LanguageCulture),
                getResource: (resourceName, languageId) => _localizationService.GetLocaleStringResourceByName(resourceName, languageId, false),
                createResource: (languageId, resourceName, resourceValue) => new LocaleStringResource { LanguageId = languageId, ResourceName = resourceName, ResourceValue = resourceValue },
                insertResource: (lsr) => _localizationService.InsertLocaleStringResource(lsr),
                updateResource: (lsr, resourceValue) => { lsr.ResourceValue = resourceValue; _localizationService.UpdateLocaleStringResource(lsr); },
                deleteResource: (lsr) => _localizationService.DeleteLocaleStringResource(lsr),
                areResourcesEqual: (lsr, resourceValue) => lsr.ResourceValue == resourceValue
#endif
            );
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
#if NOP_4_4
        public Task<IList<string>> GetWidgetZonesAsync() => Task.FromResult<IList<string>>(_widgetZones);
#else
        public IList<string> GetWidgetZones() => _widgetZones;
#endif
        List<string> _widgetZones;

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl() => $"{_webHelper.GetStoreLocation()}Admin/{UserManualsController.ControllerName}/{nameof(UserManualsController.Configure)}";

#if NOP_4_4
        private async Task CreateLocaleStringsAsync()
        {
            await ResourceHelper().CreateLocaleStringsAsync();
        }
#else
        private void CreateLocaleStrings()
        {
            ResourceHelper().CreateLocaleStrings();
        }
#endif

        /// <summary>
        /// Install plugin
        /// </summary>
#if NOP_4_4
        public async override Task InstallAsync()
#else
        public override void Install()
#endif
        {
#if NOP_4_4
            await _settingService.SaveSettingAsync(new UserManualsWidgetSettings
#else
            _settingService.SaveSetting(new UserManualsWidgetSettings
#endif
            {
                WidgetZones = "header_menu_after;productdetails_overview_bottom"
            });

#if NOP_PRE_4_3
            _objectContext.Install();
#endif

#if NOP_4_4
            await CreateLocaleStringsAsync();
            await _permissionService.InstallPermissionsAsync(new UserManualPermissionProvider());
#else
            CreateLocaleStrings();
            _permissionService.InstallPermissions(new UserManualPermissionProvider());
#endif

#if NOP_4_4
            await base.InstallAsync();
#else
            base.Install();
#endif
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
#if NOP_4_4
        public override async Task UninstallAsync()
#else
        public override void Uninstall()
#endif
        {
            //settings
#if NOP_4_4
            await _settingService.DeleteSettingAsync<UserManualsWidgetSettings>();
#else
            _settingService.DeleteSetting<UserManualsWidgetSettings>();
#endif

#if NOP_PRE_4_3
            _objectContext.Uninstall();
#endif

#if NOP_4_4
            await ResourceHelper().DeleteLocaleStringsAsync();

            // TODO in 4.4
            // _permissionService.UninstallPermissions(new UserManualPermissionProvider());

            await base.UninstallAsync();
#else
            ResourceHelper().DeleteLocaleStrings();

            _permissionService.UninstallPermissions(new UserManualPermissionProvider());

            base.Uninstall();
#endif
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone.ToLower().StartsWith("productdetails"))
            {
                return "WidgetsProductUserManuals";
            }

            return "WidgetsUserManuals";
        }

#if NOP_4_4
        public async Task ManageSiteMapAsync
#else
        public void ManageSiteMap
#endif
            (SiteMapNode rootNode)
        {
            var contentMenu = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Content Management");
            if (contentMenu == null)
            {
                // Unable to find the "Configure" menu, create our own menu container
                contentMenu = new SiteMapNode()
                {
                    SystemName = "User Manuals",
                    Title = UserManualResources.ListCaption,
                    Visible = true,
                    IconClass = "fa-cubes"
                };
                rootNode.ChildNodes.Add(contentMenu);
            }

#if NOP_4_4
            async Task<string> T(string format) => await _localizationService.GetResourceAsync(format) ?? format;
#else
            string T(string format) => _localizationService.GetResource(format) ?? format;
#endif

            foreach (var item in new List<(string caption, string controller, string action)>
            {
#if NOP_4_4
                (await T(UserManualResources.ListCaption), UserManualsController.ControllerName, nameof(UserManualsController.List)),
                (await T(AdminResources.CategoryListCaption), CategoriesController.ControllerName, nameof(CategoriesController.List)),
#else
                (T(UserManualResources.ListCaption), UserManualsController.ControllerName, nameof(UserManualsController.List)),
                (T(AdminResources.CategoryListCaption), CategoriesController.ControllerName, nameof(CategoriesController.List)),
#endif
            })
            {
                contentMenu.ChildNodes.Add(new SiteMapNode
                {
                    SystemName = $"{item.controller}.{item.action}",
                    Title = item.caption,
                    ControllerName = item.controller,
                    ActionName = item.action,
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    RouteValues = new RouteValueDictionary {
                    { "area", "Admin" }
                },
                });
            }
        }
    }
}
