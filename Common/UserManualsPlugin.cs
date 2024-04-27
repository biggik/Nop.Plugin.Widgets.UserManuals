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
using Nop.Plugin.Widgets.UserManuals.Components;

namespace Nop.Plugin.Widgets.UserManuals
{
    public class UserManualPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {
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
            IStoreContext storeContext,
            ISettingService settingService)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _languageService = languageService;
            _permissionService = permissionService;
            _settingService = settingService;
#if DEBUG
            DebugInitialize();
#endif

            // TODO remove .Result below
            var storeScope = storeContext.GetActiveStoreScopeConfigurationAsync().Result;
            var settings = _settingService.LoadSettingAsync<UserManualsWidgetSettings>(storeScope).Result;

            _widgetZones = string.IsNullOrWhiteSpace(settings.WidgetZones)
                ? new List<string>()
                : settings.WidgetZones.Split(';').ToList();
        }

#if DEBUG
        private static bool _debugInitialized = false;

        private void DebugInitialize()
        {
            if (_debugInitialized)
                return;

            _debugInitialized = true;

            ResourceHelper().CreateLocaleStringsAsync();
            _permissionService.InstallPermissionsAsync(new UserManualPermissionProvider());
        }
#endif

        private LocaleStringHelper<LocaleStringResource> ResourceHelper()
        {
            return new LocaleStringHelper<LocaleStringResource>
            (
                pluginAssembly: GetType().Assembly,
                languageCultures: from lang in _languageService.GetAllLanguagesAsync().Result select (lang.Id, lang.LanguageCulture),
                getResource: (resourceName, languageId) => _localizationService.GetLocaleStringResourceByNameAsync(resourceName, languageId, false),
                createResource: (languageId, resourceName, resourceValue) => new LocaleStringResource { LanguageId = languageId, ResourceName = resourceName, ResourceValue = resourceValue },
                insertResource: (lsr) => _localizationService.InsertLocaleStringResourceAsync(lsr),
                updateResource: (lsr, resourceValue) => { lsr.ResourceValue = resourceValue; return _localizationService.UpdateLocaleStringResourceAsync(lsr); },
                deleteResource: (lsr) => _localizationService.DeleteLocaleStringResourceAsync(lsr),
                areResourcesEqual: (lsr, resourceValue) => lsr.ResourceValue == resourceValue
            );
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public Task<IList<string>> GetWidgetZonesAsync() => Task.FromResult<IList<string>>(_widgetZones);
        List<string> _widgetZones;

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl() => $"{_webHelper.GetStoreLocation()}Admin/{UserManualsController.ControllerName}/{nameof(UserManualsController.Configure)}";

        /// <summary>
        /// Install plugin
        /// </summary>
        public async override Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new UserManualsWidgetSettings
            {
                WidgetZones = "header_menu_after;productdetails_overview_bottom"
            });

            await ResourceHelper().CreateLocaleStringsAsync();
            await _permissionService.InstallPermissionsAsync(new UserManualPermissionProvider());

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<UserManualsWidgetSettings>();


            await ResourceHelper().DeleteLocaleStringsAsync();

            // TODO in 4.4
            // _permissionService.UninstallPermissions(new UserManualPermissionProvider());

            await base.UninstallAsync();
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return widgetZone.ToLower().StartsWith("productdetails")
                ? typeof(WidgetsProductUserManualsViewComponent)
                : typeof(WidgetsUserManualsViewComponent);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
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

            async Task<string> T(string format) => await _localizationService.GetResourceAsync(format) ?? format;

            foreach (var item in new List<(string caption, string controller, string action)>
            {
                (await T(UserManualResources.ListCaption), UserManualsController.ControllerName, nameof(UserManualsController.List)),
                (await T(AdminResources.CategoryListCaption), CategoriesController.ControllerName, nameof(CategoriesController.List)),
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
